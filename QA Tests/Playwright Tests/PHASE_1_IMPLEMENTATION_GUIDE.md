# Phase 1 Implementation Guide - Detail Pages & Forms

**Date:** 2026-01-26  
**Duration:** 4-6 weeks  
**Goal:** 90-140 new tests  
**Target Coverage:** 35% → 60%

---

## 🎯 **Phase 1 Objectives**

### **Primary Goals:**
1. ✅ **Detail Page Testing** - Test all entity detail pages (Partners, Contacts, Interactions, Opportunities)
2. ✅ **Form Workflow Testing** - Test create, edit, delete workflows
3. ✅ **Form Validation Testing** - Test required fields, formats, error messages
4. ✅ **Data Persistence Testing** - Verify changes reflect across pages

### **Success Metrics:**
- 90-140 new tests created
- UI coverage increases from 35% to 60%
- All critical user workflows tested
- Zero detail page gaps
- Form workflows fully covered

---

## 📋 **Implementation Checklist**

### **Week 1-2: Infrastructure Setup**

#### **✅ Page Objects** (Days 1-3)
- [x] Create `EntityDetailPage` base class
- [x] Create `PartnerItemPage` extends EntityDetailPage
- [x] Create `ContactItemPage` extends EntityDetailPage
- [x] Create `InteractionItemPage` extends EntityDetailPage
- [x] Create `OpportunityItemPage` extends EntityDetailPage

#### **✅ Test Data Seeding** (Days 4-5)
- [x] Create `TestDataBuilder` utility
- [x] Create seed data for Partners
- [x] Create seed data for Contacts
- [x] Create seed data for Interactions
- [x] Create seed data for Opportunities
- [x] Create API helpers for seeding

#### **✅ Data-TestId Attributes** (Days 6-10)
- [ ] Add data-testid to Partner detail page
- [ ] Add data-testid to Contact detail page
- [ ] Add data-testid to Interaction detail page
- [ ] Add data-testid to Opportunity detail page
- [ ] Add data-testid to Create Partner form
- [ ] Add data-testid to Create Contact form
- [ ] Add data-testid to Create Interaction form
- [ ] Add data-testid to Create Opportunity form
- [ ] Add data-testid to Edit forms
- [ ] Add data-testid to Delete dialogs

---

### **Week 3-4: Detail Page Tests**

#### **Partner Detail Page** (Days 11-13)
- [ ] Test: Display partner information
- [ ] Test: Display partner contacts list
- [ ] Test: Display partner interactions
- [ ] Test: Display partner opportunities
- [ ] Test: Display partner documents
- [ ] Test: Edit button visibility & click
- [ ] Test: Delete button visibility & click
- [ ] Test: Workflow status display
- [ ] Test: Permission-based button visibility
- [ ] Test: Mobile responsive layout
- [ ] **Estimated:** 15-20 tests

#### **Contact Detail Page** (Days 14-16)
- [ ] Test: Display contact information
- [ ] Test: Display associated partner
- [ ] Test: Display contact interactions
- [ ] Test: Display contact documents
- [ ] Test: Edit button visibility & click
- [ ] Test: Delete button visibility & click
- [ ] Test: Workflow status display
- [ ] Test: Activity timeline
- [ ] Test: Permission-based button visibility
- [ ] Test: Mobile responsive layout
- [ ] **Estimated:** 15-20 tests

#### **Interaction Detail Page** (Days 17-19)
- [ ] Test: Display interaction details
- [ ] Test: Display participants
- [ ] Test: Display interaction notes
- [ ] Test: Display related opportunities
- [ ] Test: Display documents
- [ ] Test: Edit button visibility & click
- [ ] Test: Delete button visibility & click
- [ ] Test: Create opportunity from interaction
- [ ] Test: Permission-based button visibility
- [ ] Test: Mobile responsive layout
- [ ] **Estimated:** 12-15 tests

#### **Opportunity Detail Page** (Days 20-22)
- [ ] Test: Display opportunity header
- [ ] Test: Display opportunity tabs
- [ ] Test: Display budget information
- [ ] Test: Display schedule/timeline
- [ ] Test: Display documents
- [ ] Test: Display partnership agreements
- [ ] Test: Edit button visibility & click
- [ ] Test: Delete button visibility & click
- [ ] Test: Workflow actions
- [ ] Test: Permission-based button visibility
- [ ] Test: Mobile responsive layout
- [ ] **Estimated:** 15-20 tests

**Total Detail Page Tests:** 57-75 tests

---

### **Week 5-6: Form Workflow Tests**

#### **Create Contact Form** (Days 23-24)
- [ ] Test: Open "New Contact" dialog
- [ ] Test: Fill required fields successfully
- [ ] Test: Required field validation errors
- [ ] Test: Email format validation
- [ ] Test: Phone format validation
- [ ] Test: Submit form successfully
- [ ] Test: Verify contact appears in list
- [ ] Test: Cancel form without saving
- [ ] Test: Form field placeholders
- [ ] Test: Partner dropdown populated
- [ ] **Estimated:** 10-12 tests

#### **Edit Contact Form** (Days 25)
- [ ] Test: Open edit dialog
- [ ] Test: Form pre-populated with data
- [ ] Test: Modify contact information
- [ ] Test: Save changes successfully
- [ ] Test: Verify changes in list
- [ ] Test: Cancel without saving
- [ ] **Estimated:** 6-8 tests

#### **Delete Contact** (Day 26)
- [ ] Test: Click delete button
- [ ] Test: Confirmation dialog appears
- [ ] Test: Confirm deletion
- [ ] Test: Verify removed from list
- [ ] Test: Cancel deletion
- [ ] **Estimated:** 5 tests

#### **Create Partner Form** (Days 27-28)
- [ ] Test: Open "New Partner" dialog
- [ ] Test: Fill required fields successfully
- [ ] Test: Required field validation
- [ ] Test: Set partner type
- [ ] Test: Submit form successfully
- [ ] Test: Verify partner appears in list
- [ ] Test: Cancel without saving
- [ ] **Estimated:** 8-10 tests

#### **Edit Partner Form** (Day 29)
- [ ] Test: Open edit dialog
- [ ] Test: Form pre-populated
- [ ] Test: Modify partner information
- [ ] Test: Save changes
- [ ] Test: Cancel without saving
- [ ] **Estimated:** 5-7 tests

#### **Delete Partner** (Day 30)
- [ ] Test: Confirmation dialog
- [ ] Test: Confirm deletion
- [ ] Test: Verify removed
- [ ] Test: Cancel deletion
- [ ] **Estimated:** 4 tests

**Total Form Tests:** 38-46 tests

---

## 🏗️ **Architecture & Patterns**

### **Page Object Hierarchy**

```
BasePage (base.page.ts)
├── EntityListPage (entity-list.page.ts)
│   ├── PartnersPage
│   ├── ContactsPage
│   ├── InteractionsPage
│   └── OpportunitiesPage
│
└── EntityDetailPage (entity-detail.page.ts) ← NEW
    ├── PartnerItemPage ← NEW
    ├── ContactItemPage ← NEW
    ├── InteractionItemPage ← NEW
    └── OpportunityItemPage ← NEW
```

### **Test Data Pattern**

```typescript
// helpers/test-data-builder.ts
const testPartner = TestDataBuilder.partner()
  .withName('Test Partner')
  .withType('Organization')
  .build();

const testContact = TestDataBuilder.contact()
  .withName('John Doe')
  .withEmail('john@example.com')
  .withPartner(testPartner.id)
  .build();
```

### **Test Structure Pattern**

```typescript
// contact-item.spec.ts
test.describe('Contact Detail Page', () => {
  let contactItemPage: ContactItemPage;
  let testContact: Contact;
  
  test.beforeEach(async ({ page }) => {
    // Seed test data
    testContact = await TestDataSeeder.createContact();
    
    // Navigate to contact detail
    contactItemPage = new ContactItemPage(page);
    await loginAndNavigate(page, `/contacts/${testContact.id}`);
  });
  
  test.afterEach(async () => {
    // Cleanup test data
    await TestDataSeeder.deleteContact(testContact.id);
  });
  
  test('should display contact name', async () => {
    await contactItemPage.verifyContactName(testContact.name);
  });
});
```

---

## 📊 **Progress Tracking**

### **Week-by-Week Goals**

| Week | Focus Area | Tests Created | Coverage |
|------|------------|---------------|----------|
| Week 1 | Infrastructure Setup | 0 | 35% |
| Week 2 | Data-TestId Attributes | 0 | 35% |
| Week 3 | Partner & Contact Detail | 30-40 | 42% |
| Week 4 | Interaction & Opportunity Detail | 27-35 | 50% |
| Week 5 | Form Workflows (Create/Edit) | 25-30 | 56% |
| Week 6 | Form Workflows (Delete) & Cleanup | 8-15 | 60% |
| **Total** | **Phase 1 Complete** | **90-140** | **60%** |

---

## 🚀 **Quick Start Guide**

### **Step 1: Set Up Your Environment**

```bash
# Install dependencies
cd "Playwright Tests"
npm install

# Set up test database (if needed)
npm run db:seed

# Verify Playwright is working
npx playwright test --list
```

### **Step 2: Run Sample Detail Page Test**

```bash
# Run partner detail tests
npx playwright test partner-item.spec.ts --project=chromium

# Run with UI mode to see tests execute
npx playwright test partner-item.spec.ts --ui
```

### **Step 3: Add Data-TestId Attributes**

Follow the guide in `DATA_TESTID_GUIDE.md` to add attributes to Angular components.

### **Step 4: Create Test Data**

```typescript
// Import the test data builder
import { TestDataBuilder, TestDataSeeder } from './helpers/test-data-builder';

// Create test data in your test
const partner = await TestDataSeeder.createPartner({
  name: 'Test Partner Org',
  type: 'Organization'
});

// Use in test
await loginAndNavigate(page, `/partners/${partner.id}`);

// Cleanup after test
await TestDataSeeder.deletePartner(partner.id);
```

---

## 🔧 **Troubleshooting**

### **Common Issues:**

**Issue:** "Element not found with data-testid"
**Solution:** Verify data-testid attributes added to Angular component. Check `DATA_TESTID_GUIDE.md`.

**Issue:** "Test data not persisting"
**Solution:** Check API mocks are properly configured. See `api-mocks.helper.ts`.

**Issue:** "Navigation timeout on detail page"
**Solution:** Add page-specific waits. See webkit fixes in `auth.helper.ts` for patterns.

**Issue:** "Permission errors in tests"
**Solution:** Verify API mock returns correct permissions. Check `api-mocks.helper.ts`.

---

## 📚 **Key Files Reference**

| File | Purpose |
|------|---------|
| `pages/entity-detail.page.ts` | Base class for detail pages |
| `pages/partner-item.page.ts` | Partner detail page object |
| `pages/contact-item.page.ts` | Contact detail page object |
| `helpers/test-data-builder.ts` | Test data creation utilities |
| `helpers/test-data-seeder.ts` | API-based data seeding |
| `DATA_TESTID_GUIDE.md` | Guide for adding data-testid attributes |
| `tests/partner-item.spec.ts` | Partner detail page tests |
| `tests/contact-item.spec.ts` | Contact detail page tests |

---

## ✅ **Definition of Done**

A test is considered complete when:

- [ ] Test passes on all 3 browsers (Chromium, Firefox, Webkit)
- [ ] Test uses Page Object Model pattern
- [ ] Test uses data-testid selectors
- [ ] Test has proper setup/teardown
- [ ] Test has descriptive name
- [ ] Test verifies expected behavior
- [ ] Test handles permissions correctly
- [ ] Test is documented with JSDoc comments
- [ ] Test follows existing patterns
- [ ] Test is committed to git

---

## 🎯 **Success Criteria**

Phase 1 is considered successful when:

- ✅ All 4 detail page test suites created (Partner, Contact, Interaction, Opportunity)
- ✅ 90-140 new tests implemented and passing
- ✅ UI coverage increased from 35% to 60%
- ✅ All detail pages have data-testid attributes
- ✅ All forms have data-testid attributes
- ✅ Test data seeding infrastructure works reliably
- ✅ Tests run in CI/CD pipeline
- ✅ Zero critical bugs discovered in implementation
- ✅ Test execution time < 15 minutes for full suite

---

## 📞 **Need Help?**

- **Technical Questions:** Review existing test patterns in `contacts.spec.ts`
- **Page Object Questions:** See `entity-list.page.ts` for patterns
- **Data-TestId Questions:** Review `DATA_TESTID_GUIDE.md`
- **Test Data Questions:** See `test-data-builder.ts` examples

---

**Document Owner:** QA Team  
**Last Updated:** 2026-01-26  
**Status:** Ready for Implementation  
**Next Review:** Weekly progress meetings
