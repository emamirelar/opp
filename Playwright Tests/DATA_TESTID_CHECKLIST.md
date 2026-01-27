# Data-TestId Quick Checklist

**For Developers:** Print this and keep it handy while adding test attributes!

---

## ✅ **Partner Detail Page** (`partner-item.component.html`)

### **Page Structure:**
- [ ] `partner-detail-header` - Page header container
- [ ] `partner-title` - Partner name display
- [ ] `partner-type` - Partner type field
- [ ] `partner-status` - Partner status badge
- [ ] `partner-description` - Partner description text
- [ ] `partner-website` - Partner website link

### **Action Buttons:**
- [ ] `edit-partner-button` - Edit button
- [ ] `delete-partner-button` - Delete button
- [ ] `back-to-list-button` - Back button (optional)
- [ ] `view-partner-tree-button` - Partner tree button (optional)

### **Sections:**
- [ ] `partner-contacts-section` - Contacts section container
- [ ] `partner-interactions-section` - Interactions section container
- [ ] `partner-opportunities-section` - Opportunities section container
- [ ] `partner-documents` - Documents section
- [ ] `partner-activity-timeline` - Activity timeline

### **Section Items:**
- [ ] `partner-contact-item` - Each contact in list
- [ ] `partner-interaction-item` - Each interaction in list
- [ ] `partner-opportunity-item` - Each opportunity in list
- [ ] `partner-document-item` - Each document in list

---

## ✅ **Contact Detail Page** (`contact-item.component.html`)

### **Page Structure:**
- [ ] `contact-detail-header` - Page header container
- [ ] `contact-title` - Contact name display
- [ ] `contact-name` - Contact name field
- [ ] `contact-email` - Contact email field
- [ ] `contact-phone` - Contact phone field
- [ ] `contact-title` - Contact job title field
- [ ] `contact-partner` - Associated partner display
- [ ] `contact-department` - Department field

### **Action Buttons:**
- [ ] `edit-contact-button` - Edit button
- [ ] `delete-contact-button` - Delete button

### **Sections:**
- [ ] `contact-interactions-section` - Interactions section
- [ ] `contact-opportunities-section` - Opportunities section (optional)
- [ ] `contact-documents` - Documents section
- [ ] `contact-activity-timeline` - Activity timeline

### **Section Items:**
- [ ] `contact-interaction-item` - Each interaction in list
- [ ] `contact-document-item` - Each document in list

---

## ✅ **Interaction Detail Page** (`interaction-item.component.html`)

### **Page Structure:**
- [ ] `interaction-detail-header` - Page header
- [ ] `interaction-type` - Interaction type field
- [ ] `interaction-date` - Interaction date field
- [ ] `interaction-description` - Description/notes field
- [ ] `interaction-location` - Location field

### **Action Buttons:**
- [ ] `edit-interaction-button` - Edit button
- [ ] `delete-interaction-button` - Delete button
- [ ] `create-opportunity-from-interaction-button` - Create opportunity button

### **Sections:**
- [ ] `interaction-participants-section` - Participants section
- [ ] `interaction-opportunities-section` - Related opportunities
- [ ] `interaction-documents` - Documents section

### **Section Items:**
- [ ] `interaction-participant-item` - Each participant
- [ ] `interaction-opportunity-item` - Each related opportunity

---

## ✅ **Opportunity Detail Page** (`opportunity-item.component.html`)

### **Page Structure:**
- [ ] `opportunity-detail-header` - Page header
- [ ] `opportunity-title` - Opportunity title field
- [ ] `opportunity-value` - Opportunity value field
- [ ] `opportunity-stage` - Current stage badge
- [ ] `opportunity-start-date` - Start date field
- [ ] `opportunity-end-date` - End date field
- [ ] `opportunity-description` - Description field

### **Action Buttons:**
- [ ] `edit-opportunity-button` - Edit button
- [ ] `delete-opportunity-button` - Delete button
- [ ] `submit-opportunity-button` - Submit button
- [ ] `approve-opportunity-button` - Approve button
- [ ] `activate-opportunity-button` - Activate button

### **Sections:**
- [ ] `opportunity-budget-section` - Budget section
- [ ] `opportunity-schedule-section` - Schedule/timeline section
- [ ] `opportunity-partners-section` - Partners section
- [ ] `opportunity-contacts-section` - Contacts section
- [ ] `opportunity-interactions-section` - Interactions section
- [ ] `opportunity-dst-section` - DST section (optional)
- [ ] `opportunity-workflow-actions` - Workflow toolbar
- [ ] `opportunity-documents` - Documents section

### **Section Items:**
- [ ] `opportunity-partner-item` - Each partner
- [ ] `opportunity-contact-item` - Each contact
- [ ] `opportunity-interaction-item` - Each interaction

---

## ✅ **Create Partner Form** (`new-partner.component.html`)

### **Form Structure:**
- [ ] `create-partner-dialog` - Dialog container
- [ ] `create-partner-form` - Form element

### **Form Fields:**
- [ ] `partner-name-input` - Name input
- [ ] `partner-type-select` - Type dropdown
- [ ] `partner-status-select` - Status dropdown (optional)
- [ ] `partner-description-textarea` - Description textarea
- [ ] `partner-website-input` - Website input

### **Validation Errors:**
- [ ] `partner-name-error` - Name validation message
- [ ] `partner-type-error` - Type validation message

### **Form Buttons:**
- [ ] `submit-partner-button` - Submit/Create button
- [ ] `cancel-partner-button` - Cancel button

---

## ✅ **Create Contact Form** (`new-contact.component.html`)

### **Form Structure:**
- [ ] `create-contact-dialog` - Dialog container
- [ ] `create-contact-form` - Form element

### **Form Fields:**
- [ ] `contact-name-input` - Name input
- [ ] `contact-email-input` - Email input
- [ ] `contact-phone-input` - Phone input
- [ ] `contact-title-input` - Job title input
- [ ] `contact-partner-select` - Partner dropdown
- [ ] `contact-department-input` - Department input

### **Validation Errors:**
- [ ] `contact-name-error` - Name validation
- [ ] `contact-email-error` - Email validation
- [ ] `contact-partner-error` - Partner validation

### **Form Buttons:**
- [ ] `submit-contact-button` - Submit/Create button
- [ ] `cancel-contact-button` - Cancel button

---

## ✅ **Create Interaction Form** (`new-interaction.component.html`)

### **Form Structure:**
- [ ] `create-interaction-dialog` - Dialog container
- [ ] `create-interaction-form` - Form element

### **Form Fields:**
- [ ] `interaction-type-select` - Type dropdown
- [ ] `interaction-date-input` - Date picker
- [ ] `interaction-location-input` - Location input
- [ ] `interaction-description-textarea` - Description textarea
- [ ] `interaction-participants-multiselect` - Participants multi-select

### **Validation Errors:**
- [ ] `interaction-type-error` - Type validation
- [ ] `interaction-date-error` - Date validation

### **Form Buttons:**
- [ ] `submit-interaction-button` - Submit/Create button
- [ ] `cancel-interaction-button` - Cancel button

---

## ✅ **Create Opportunity Form** (`create-opportunity.component.html`)

### **Form Structure:**
- [ ] `create-opportunity-dialog` - Dialog container
- [ ] `create-opportunity-form` - Form element

### **Form Fields:**
- [ ] `opportunity-title-input` - Title input
- [ ] `opportunity-value-input` - Value/amount input
- [ ] `opportunity-start-date-input` - Start date picker
- [ ] `opportunity-end-date-input` - End date picker
- [ ] `opportunity-description-textarea` - Description textarea
- [ ] `opportunity-partners-multiselect` - Partners multi-select
- [ ] `opportunity-contacts-multiselect` - Contacts multi-select

### **Validation Errors:**
- [ ] `opportunity-title-error` - Title validation
- [ ] `opportunity-value-error` - Value validation

### **Form Buttons:**
- [ ] `submit-opportunity-button` - Submit/Create button
- [ ] `cancel-opportunity-button` - Cancel button

---

## ✅ **Delete Confirmation Dialogs**

### **Delete Partner Dialog:**
- [ ] `delete-partner-dialog` - Dialog container
- [ ] `dialog-title` - Dialog title
- [ ] `dialog-message` - Confirmation message
- [ ] `confirm-delete-button` - Confirm button
- [ ] `cancel-delete-button` - Cancel button

### **Delete Contact Dialog:**
- [ ] `delete-contact-dialog` - Dialog container
- [ ] `confirm-delete-button` - Confirm button
- [ ] `cancel-delete-button` - Cancel button

### **Delete Interaction Dialog:**
- [ ] `delete-interaction-dialog` - Dialog container
- [ ] `confirm-delete-button` - Confirm button
- [ ] `cancel-delete-button` - Cancel button

### **Delete Opportunity Dialog:**
- [ ] `delete-opportunity-dialog` - Dialog container
- [ ] `confirm-delete-button` - Confirm button
- [ ] `cancel-delete-button` - Cancel button

---

## 📋 **General Rules**

### **Naming Pattern:**
```
[entity-name]-[element-type]
[entity-name]-[section-name]-[element-type]
```

### **Entity Names:**
- `partner` - Partner/Organization
- `contact` - Contact/Person
- `interaction` - Interaction/Meeting
- `opportunity` - Business Opportunity

### **Element Types:**
- `-header` - Page/section headers
- `-title` - Title/name displays
- `-button` - Action buttons
- `-input` - Text inputs
- `-select` - Dropdowns
- `-textarea` - Text areas
- `-error` - Validation messages
- `-section` - Content sections
- `-item` - List items
- `-dialog` - Dialog/modal containers

### **Common Buttons:**
- `new-{entity}-button`
- `edit-{entity}-button`
- `delete-{entity}-button`
- `submit-{entity}-button`
- `cancel-{entity}-button`
- `confirm-delete-button`

---

## 🎯 **Quick Tips**

1. ✅ **Use kebab-case** (lowercase with hyphens)
2. ✅ **Be specific** (include entity name)
3. ✅ **Be consistent** (follow the pattern)
4. ✅ **Test it** (verify attribute is visible in browser DevTools)

---

## 🔍 **How to Verify**

1. Run the Angular app: `npm start`
2. Open browser DevTools (F12)
3. Find the element in Elements tab
4. Look for `data-testid="attribute-name"` in HTML

---

**Print this checklist and mark items as you add them!**

**Reference:** See `DATA_TESTID_GUIDE.md` for detailed examples and patterns.
