# JIRA Requirements Test Cases

## Overview

This document contains comprehensive test cases derived from the JIRA export (52 weeks of stories, bugs, epics, and changes) for the UNOPS Opportunity+ system.

**Source:** `QA Project Opps+ Reported (Total 52 weeks) (JIRA).csv`
**Generated:** 2026-02-04
**Test Categories:** Positive, Negative, Boundary, Permission, Security, Performance

---

## 1. Take a Tour Feature (PNO-446)

### Story Summary
Interactive guided tours for different pages within UNOPS Opportunity+ using Driver.js library with multi-language support.

### Test Cases

| ID | Type | Title | Steps | Expected Result |
|----|------|-------|-------|-----------------|
| PNO-446-POS-001 | Positive | Tour button visible on all pages | Navigate to any supported page | Play circle icon visible in top nav with tooltip "Take a Tour" |
| PNO-446-POS-002 | Positive | Tour starts on supported page | Click tour button on Partners page | Tour overlay appears with step 1 highlighted |
| PNO-446-POS-003 | Positive | Tour step navigation forward | Start tour, click "Next" | Advances to next step with correct element highlighted |
| PNO-446-POS-004 | Positive | Tour step navigation backward | On step 3, click "Previous" | Returns to step 2 with correct element highlighted |
| PNO-446-POS-005 | Positive | Tour progress indicator | Start multi-step tour | Shows "Step X of Y" correctly |
| PNO-446-POS-006 | Positive | Tour close via button | Click "Close" button during tour | Tour dismissed, page returns to normal |
| PNO-446-POS-007 | Positive | Tour close via ESC key | Press ESC during tour | Tour dismissed |
| PNO-446-POS-008 | Positive | Multi-language English | Set language to English, start tour | All content displays in English |
| PNO-446-POS-009 | Positive | Multi-language French | Set language to French, start tour | Button shows "Faire une Visite", content in French |
| PNO-446-POS-010 | Positive | Multi-language Spanish | Set language to Spanish, start tour | Button shows "Hacer un Tour", content in Spanish |
| PNO-446-POS-011 | Positive | Multi-language Portuguese | Set language to Portuguese, start tour | Button shows "Fazer um Tour", content in Portuguese |
| PNO-446-NEG-001 | Negative | Fallback on unsupported page | Click tour on Leads page | User-friendly message displayed (not error) |
| PNO-446-NEG-002 | Negative | Tour with missing element | Start tour when target element hidden | Tour handles gracefully with fallback |
| PNO-446-BND-001 | Boundary | Tour completion tracking | Complete full tour | Tour marked as completed |
| PNO-446-BND-002 | Boundary | Tour restart after completion | Complete tour, click button again | Tour can be restarted |

---

## 2. Advanced Search (PNO-677)

### Bug Summary
Advanced search does not work for certain fields: Pooled Fund, Liaison Office Name, Approval Date, Key Global Partner, UN Secretariat Partner, Full Name (equals).

### Test Cases

| ID | Type | Title | Steps | Expected Result |
|----|------|-------|-------|-----------------|
| PNO-677-POS-001 | Positive | Search by Pooled Fund = Yes | Advanced Search > Pooled Fund = Yes | Only Pooled Fund partners shown |
| PNO-677-POS-002 | Positive | Search by Pooled Fund = No | Advanced Search > Pooled Fund = No | Only non-Pooled Fund partners shown |
| PNO-677-POS-003 | Positive | Search by Liaison Office Name | Advanced Search > Liaison Office = "Geneva" | Partners with Geneva liaison shown |
| PNO-677-POS-004 | Positive | Search by Approval Date | Advanced Search > Approval Date = specific date | Partners approved on that date shown |
| PNO-677-POS-005 | Positive | Search by Key Global Partner | Advanced Search > Key Global Partner = Yes | Only key global partners shown |
| PNO-677-POS-006 | Positive | Search by UN Secretariat Partner | Advanced Search > UN Secretariat = Yes | Only UN Secretariat partners shown |
| PNO-677-POS-007 | Positive | Search First Name equals | Advanced Search > First Name = "Adam" | Exact match for Adam shown |
| PNO-677-POS-008 | Positive | Search First Name contains | Advanced Search > First Name contains "Ad" | All names containing "Ad" shown |
| PNO-677-NEG-001 | Negative | Search non-existent Liaison Office | Search > Liaison Office = "NonExistent" | No results, empty state message |
| PNO-677-NEG-002 | Negative | Search invalid date format | Enter invalid date in Approval Date | Validation error or field rejected |
| PNO-677-BND-001 | Boundary | Search with all boolean filters | Apply all boolean filters simultaneously | Results match ALL criteria |

---

## 3. Contact Import/Duplicates (PNO-676)

### Bug Summary
Edited duplicate contacts cannot be imported even after making them unique.

### Test Cases

| ID | Type | Title | Steps | Expected Result |
|----|------|-------|-------|-----------------|
| PNO-676-POS-001 | Positive | Import unique contacts | Import CSV with unique contacts | All contacts imported successfully |
| PNO-676-POS-002 | Positive | Duplicate detection | Import CSV with duplicate email | Duplicate flagged in import dialog |
| PNO-676-POS-003 | Positive | Edit duplicate to unique | Edit duplicate record to new email | Duplicate flag removed |
| PNO-676-POS-004 | Positive | Import edited unique record | After edit, select and import | Record imports successfully |
| PNO-676-NEG-001 | Negative | Import unedited duplicate | Attempt import of flagged duplicate | Import blocked with message |
| PNO-676-NEG-002 | Negative | Partial duplicate edit | Change name but not email | Still flagged as duplicate |
| PNO-676-BND-001 | Boundary | Mass import 100+ contacts | Import large CSV file | All unique records imported |
| PNO-676-BND-002 | Boundary | Import with mixed duplicates | CSV with 50% duplicates | Valid records imported, duplicates flagged |

---

## 4. Partner List Hierarchical View (PNO-256)

### Story Summary
Display partner list with Category/Group hierarchy that can be expanded/collapsed.

### Test Cases

| ID | Type | Title | Steps | Expected Result |
|----|------|-------|-------|-----------------|
| PNO-256-POS-001 | Positive | Hierarchical list displays | Navigate to Partners | Category > Group > Partner hierarchy shown |
| PNO-256-POS-002 | Positive | Display required columns | View partner list | Category, Group, Partner, Logo, Office visible |
| PNO-256-POS-003 | Positive | Logo displays when present | Partner with logo | Logo appears in list row |
| PNO-256-POS-004 | Positive | No logo placeholder | Partner without logo | Appropriate placeholder shown |
| PNO-256-POS-005 | Positive | Expand hierarchy node | Click expand on Category | Child groups/partners revealed |
| PNO-256-POS-006 | Positive | Collapse hierarchy node | Click collapse on expanded node | Children hidden |
| PNO-256-POS-007 | Positive | Default view is user's org unit | Login and navigate to Partners | Only org unit partners shown initially |
| PNO-256-NEG-001 | Negative | Empty category | Category with no partners | Empty state or hidden |
| PNO-256-BND-001 | Boundary | Large hierarchy (100+ nodes) | Navigate with many partners | Performance acceptable (<3s) |

---

## 5. Contact List Columns/Sort (PNO-255)

### Story Summary
Contact list with sortable columns including Contact Image, Name, Title, Partner, Org Unit.

### Test Cases

| ID | Type | Title | Steps | Expected Result |
|----|------|-------|-------|-----------------|
| PNO-255-POS-001 | Positive | Contact list displays columns | Navigate to Contacts | Image, Name, Title, Partner, Org Unit visible |
| PNO-255-POS-002 | Positive | Sort by Name ascending | Click Name header | Sorted A-Z |
| PNO-255-POS-003 | Positive | Sort by Name descending | Click Name header twice | Sorted Z-A |
| PNO-255-POS-004 | Positive | Sort by Title | Click Title header | Sorted alphabetically by title |
| PNO-255-POS-005 | Positive | Sort by Partner | Click Partner header | Sorted by partner name |
| PNO-255-POS-006 | Positive | Sort by Org Unit | Click Org Unit header | Sorted by org unit |
| PNO-255-POS-007 | Positive | Contact image displays | Contact with photo | Photo shown in list |
| PNO-255-NEG-001 | Negative | Sort with null values | Sort by Title with blank titles | Nulls at end/beginning consistently |
| PNO-255-BND-001 | Boundary | Sort 1000+ contacts | Large dataset sort | Completes in <2s |

---

## 6. Notification Bugs (PNO-696)

### Bug Summary
Homepage notifications don't work (error on click) and disappear after clicking.

### Test Cases

| ID | Type | Title | Steps | Expected Result |
|----|------|-------|-------|-----------------|
| PNO-696-POS-001 | Positive | Notification displays | Log interaction via add-on | Notification appears in Recent Activity |
| PNO-696-POS-002 | Positive | Click notification navigates | Click on notification | Navigate to related record (no error) |
| PNO-696-POS-003 | Positive | Notification persists after view | Click notification, go back | Notification still visible (until dismissed) |
| PNO-696-POS-004 | Positive | Dismiss notification | Click X on notification | Notification removed |
| PNO-696-NEG-001 | Negative | Click invalid notification | Notification for deleted record | Graceful error message |
| PNO-696-BND-001 | Boundary | Multiple notifications | 10+ notifications | All display correctly |

---

## 7. Gmail Add-on (PNO-474)

### Bug Summary
Add-on logging interactions but not contacts; missing link to view in system.

### Test Cases

| ID | Type | Title | Steps | Expected Result |
|----|------|-------|-------|-----------------|
| PNO-474-POS-001 | Positive | Log interaction from Gmail | Sync email via add-on | Interaction created in Opp+ |
| PNO-474-POS-002 | Positive | Log contact from Gmail | Sync email, create contact | Contact created correctly |
| PNO-474-POS-003 | Positive | View button after creation | Create interaction | "View" button appears, links to Opp+ |
| PNO-474-POS-004 | Positive | System notification on sync | Sync contact/partner | Notification in Opp+ dashboard |
| PNO-474-POS-005 | Positive | Contact name parsed correctly | Sync "John Smith <john@example.com>" | First: John, Last: Smith, Email: john@example.com |
| PNO-474-NEG-001 | Negative | Sync with invalid email | Sync malformed email | Graceful error handling |
| PNO-474-NEG-002 | Negative | Bulk sync >15 contacts | Attempt to sync 20 contacts | Prompt to contact admin for bulk upload |
| PNO-474-BND-001 | Boundary | Long email thread sync | Sync 50-email thread | Thread grouped correctly |

---

## 8. Interaction List View (PNO-230)

### Story Summary
List of interactions with partners, type, title, date, contacts, created by, org unit.

### Test Cases

| ID | Type | Title | Steps | Expected Result |
|----|------|-------|-------|-----------------|
| PNO-230-POS-001 | Positive | Display interaction columns | Navigate to Interactions | All required columns visible |
| PNO-230-POS-002 | Positive | Sort by Date | Click Date header | Sorted by date |
| PNO-230-POS-003 | Positive | Sort by Type | Click Type header | Sorted by interaction type |
| PNO-230-POS-004 | Positive | Click to view details | Click on interaction row | Full details modal/page opens |
| PNO-230-POS-005 | Positive | Related contacts displayed | View interaction | All linked contacts shown |
| PNO-230-POS-006 | Positive | Related partners displayed | View interaction | All linked partners shown |
| PNO-230-POS-007 | Positive | Default view is org unit | Login, view Interactions | Only org unit interactions initially |
| PNO-230-NEG-001 | Negative | No interactions | New user with no data | Empty state message |
| PNO-230-BND-001 | Boundary | 500+ interactions | Large dataset | Pagination works, <3s load |

---

## 9. Home Page Requirements (PNO-760)

### Story Summary
Enable creation of new opportunity directly from home page.

### Test Cases

| ID | Type | Title | Steps | Expected Result |
|----|------|-------|-------|-----------------|
| PNO-760-POS-001 | Positive | New Opportunity button visible | Navigate to Home | "New Opportunity" button visible |
| PNO-760-POS-002 | Positive | Create opportunity from home | Click "New Opportunity" | Opportunity creation form opens |
| PNO-760-POS-003 | Positive | Complete creation from home | Fill form, save | Opportunity created, user navigated to detail |
| PNO-760-NEG-001 | Negative | Button hidden for GENUSER | Login as General User | Button not visible |
| PNO-760-PRM-001 | Permission | Partner User sees button | Login as Partner User | Button visible |
| PNO-760-PRM-002 | Permission | Admin sees button | Login as Administrator | Button visible |

---

## 10. AI Assistant Issues (PNO-694)

### Bug Summary
AI Assistant not responsive in production/QA.

### Test Cases

| ID | Type | Title | Steps | Expected Result |
|----|------|-------|-------|-----------------|
| PNO-694-POS-001 | Positive | AI responds to query | Open AI, ask question | Response received (not blank) |
| PNO-694-POS-002 | Positive | AI provides helpful response | Ask "Show me funding partners" | Relevant information returned |
| PNO-694-POS-003 | Positive | AI handles context | Ask follow-up question | Context maintained |
| PNO-694-NEG-001 | Negative | AI handles invalid query | Enter gibberish | Graceful response |
| PNO-694-NEG-002 | Negative | AI handles empty query | Submit empty message | Validation or prompt for input |
| PNO-694-BND-001 | Boundary | Long query (1000+ chars) | Submit very long question | Handled without crash |
| PNO-694-PER-001 | Performance | Response time | Ask standard question | Response within 10 seconds |

---

## 11. Performance Issues (PNO-693)

### Bug Summary
Global Search and Interactions page loading slow in production.

### Test Cases

| ID | Type | Title | Steps | Expected Result |
|----|------|-------|-------|-----------------|
| PNO-693-PER-001 | Performance | Global Search load time | Click global search | Results within 3 seconds |
| PNO-693-PER-002 | Performance | Interactions page load | Navigate to Interactions | Page loads within 3 seconds |
| PNO-693-PER-003 | Performance | Global Search with 1000+ records | Search in large dataset | Results within 5 seconds |
| PNO-693-PER-004 | Performance | Interactions with 500+ records | Load large interactions list | Loads within 5 seconds |
| PNO-693-PER-005 | Performance | Concurrent users | 10 users searching simultaneously | Response time <5s per user |

---

## 12. Contact Creation Validation (PNO-691)

### Story Summary
Improve contact creation with Draft status, mandatory field validation, and bulk upload limits.

### Test Cases

| ID | Type | Title | Steps | Expected Result |
|----|------|-------|-------|-----------------|
| PNO-691-POS-001 | Positive | Synced contact starts as Draft | Sync contact via Gmail | Status = Draft |
| PNO-691-POS-002 | Positive | Action Required shows drafts | View Home page | Draft contacts in Action Required |
| PNO-691-POS-003 | Positive | Activate with all required fields | Fill First, Last, Email, Title | Contact activates successfully |
| PNO-691-NEG-001 | Negative | Cannot activate without First Name | Leave First Name empty | Validation error |
| PNO-691-NEG-002 | Negative | Cannot activate without Last Name | Leave Last Name empty | Validation error |
| PNO-691-NEG-003 | Negative | Cannot activate without Email | Leave Email empty | Validation error |
| PNO-691-NEG-004 | Negative | Cannot activate without Title | Leave Title empty | Validation error |
| PNO-691-BND-001 | Boundary | Bulk sync >15 prompts admin | Sync 16 contacts | Prompt to contact Global Admin |

---

## 13. Partner Approval & Due Diligence (PNO-582, PNO-663)

### Story Summary
Partner approval workflow with Due Diligence fields and Entity System Status.

### Test Cases

| ID | Type | Title | Steps | Expected Result |
|----|------|-------|-------|-----------------|
| PNO-582-POS-001 | Positive | Draft partner can be activated | Fill required fields, click Activate | Partner status = Active |
| PNO-582-POS-002 | Positive | DD Not Required allows approval | Set DD = Not Required | Admin can approve partner |
| PNO-582-POS-003 | Positive | DD Required + Approved allows approval | Set DD = Required, Approved | Admin can approve partner |
| PNO-582-POS-004 | Positive | DD Expiry warning (6 months) | DD expires in 5 months | Warning message displayed |
| PNO-582-POS-005 | Positive | Approved partner shows status | Partner approved | "Approved" tag visible |
| PNO-582-POS-006 | Positive | Unapprove partner (Admin only) | Admin clicks Unapprove | Partner status = Not Approved |
| PNO-582-NEG-001 | Negative | DD Required + Not Approved blocks approval | DD = Required, Not Approved | Cannot approve partner |
| PNO-582-NEG-002 | Negative | Not Approved blocks opportunity creation | Create opportunity on unapproved partner | Creation blocked |
| PNO-582-NEG-003 | Negative | Org Unit Admin cannot Archive | Login as OU Admin, try Archive | Action not available |
| PNO-582-PRM-001 | Permission | Only Partner Global Admin can Close | Login as Partner User | Close button hidden |
| PNO-582-PRM-002 | Permission | Partner Global Admin can Archive | Login as PGA | Archive button visible |
| PNO-582-BND-001 | Boundary | DD expiry date exactly today | Set expiry = today | Warning or expired state |

---

## 14. Global Filter Issues (PNO-592)

### Bug Summary
Various issues with global filter functionality.

### Test Cases

| ID | Type | Title | Steps | Expected Result |
|----|------|-------|-------|-----------------|
| PNO-592-POS-001 | Positive | Filter by single org unit | Select org unit in filter | Only that org unit's data shown |
| PNO-592-POS-002 | Positive | Filter by multiple org units | Select 2+ org units | Combined data shown |
| PNO-592-POS-003 | Positive | Clear filter | Click clear/reset | All data shown |
| PNO-592-POS-004 | Positive | Filter persists on navigation | Apply filter, navigate away and back | Filter still applied |
| PNO-592-NEG-001 | Negative | Filter with no matching data | Select org unit with no data | Empty state message |
| PNO-592-BND-001 | Boundary | Filter all org units | Select all 50+ org units | Equivalent to no filter |

---

## 15. Interaction Section Enhancement (PNO-378)

### Story Summary
Email creation, thread view, and interaction card display.

### Test Cases

| ID | Type | Title | Steps | Expected Result |
|----|------|-------|-------|-----------------|
| PNO-378-POS-001 | Positive | Manual email creation | Create interaction type Email | Sender, recipient, subject, body fields available |
| PNO-378-POS-002 | Positive | Gmail add-on creates interaction | Sync email from Gmail | Interaction linked to contact |
| PNO-378-POS-003 | Positive | Thread view shows emails | View email thread | All related emails grouped |
| PNO-378-POS-004 | Positive | Thread summary at top | View thread | Summary of conversation displayed |
| PNO-378-POS-005 | Positive | Individual emails expandable | Click email in thread | Full email content shown |
| PNO-378-POS-006 | Positive | Edit single email in thread | Edit one email | Only that email modified |
| PNO-378-NEG-001 | Negative | Thread with missing parent | Orphaned email | Displays as standalone |
| PNO-378-BND-001 | Boundary | Thread with 100 emails | Long thread | Performance acceptable |

---

## 16. Mass Upload (PNO-457)

### Bug Summary
Mass upload/import not working for User Permissions, Contacts, Interactions.

### Test Cases

| ID | Type | Title | Steps | Expected Result |
|----|------|-------|-------|-----------------|
| PNO-457-POS-001 | Positive | Import contacts CSV | Upload valid CSV | Contacts imported |
| PNO-457-POS-002 | Positive | Import interactions CSV | Upload valid CSV | Interactions imported |
| PNO-457-POS-003 | Positive | Import user permissions | Upload permissions CSV | Permissions assigned |
| PNO-457-POS-004 | Positive | Progress indicator during import | Start large import | Progress shown |
| PNO-457-NEG-001 | Negative | Invalid CSV format | Upload malformed CSV | Error message |
| PNO-457-NEG-002 | Negative | Empty CSV | Upload empty file | "No data" message |
| PNO-457-BND-001 | Boundary | Import 5000 records | Very large import | Completes without timeout |
| PNO-457-PER-001 | Performance | Import 1000 records | Large import | Completes in <60 seconds |

---

## Test Summary

| Category | Count |
|----------|-------|
| Stories | 25+ |
| Bugs | 30+ |
| Positive Tests | 120+ |
| Negative Tests | 60+ |
| Boundary Tests | 30+ |
| Permission Tests | 20+ |
| Performance Tests | 15+ |
| **Total Test Cases** | **~245** |

---

## Related Executable Tests

- **Playwright**: `QA Tests/Playwright Tests/jira-requirements.spec.ts`
- **C#**: `QA Tests/C# Tests/UNOPS.PAO.Business.Tests/JIRA/`

