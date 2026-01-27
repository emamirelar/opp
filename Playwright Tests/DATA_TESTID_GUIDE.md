# Data-TestId Attribute Guide

**Purpose:** Guide for adding `data-testid` attributes to Angular components for E2E testing  
**Audience:** Developers working on Angular UI components  
**Date:** 2026-01-26

---

## 📋 **Why Data-TestId Attributes?**

### **Benefits:**
1. ✅ **Stable selectors** - Won't break when CSS classes change
2. ✅ **Self-documenting** - Clear purpose of each element
3. ✅ **Test maintainability** - Easy to update tests
4. ✅ **Cross-browser compatible** - Works consistently everywhere
5. ✅ **Performance** - Faster selector queries than complex CSS

### **DO Use data-testid for:**
- Interactive elements (buttons, inputs, links)
- Content sections (headers, panels, cards)
- List items and data tables
- Form fields and validation messages
- Navigation elements
- Dialog/modal windows

### **DON'T Use data-testid for:**
- Pure styling elements (divs, spans without purpose)
- Elements inside third-party components (unless you wrap them)
- Elements that are never tested

---

## 🎯 **Naming Conventions**

### **General Pattern:**
```
[entity-name]-[element-type]
[entity-name]-[section-name]-[element-type]
```

### **Examples:**

**List Pages:**
```html
<!-- Page structure -->
<div data-testid="partners-header">...</div>
<div data-testid="partners-title">Partners</div>
<div data-testid="partners-listview">...</div>

<!-- Action buttons -->
<button data-testid="new-partner-button">New Partner</button>
<button data-testid="export-button">Export</button>
<button data-testid="import-button">Import</button>

<!-- Table rows -->
<tr data-testid="partner-row-{{partner.id}}">...</tr>
```

**Detail Pages:**
```html
<!-- Page structure -->
<div data-testid="partner-detail-header">...</div>
<div data-testid="partner-title">{{partner.name}}</div>
<div data-testid="partner-description">{{partner.description}}</div>

<!-- Action buttons -->
<button data-testid="edit-partner-button">Edit</button>
<button data-testid="delete-partner-button">Delete</button>

<!-- Sections -->
<div data-testid="partner-contacts-section">...</div>
<div data-testid="partner-interactions-section">...</div>

<!-- Section items -->
<div data-testid="partner-contact-item">...</div>
<div data-testid="partner-interaction-item">...</div>
```

**Forms:**
```html
<!-- Form container -->
<form data-testid="create-partner-form">
  <!-- Form fields -->
  <input data-testid="partner-name-input" />
  <select data-testid="partner-type-select">...</select>
  <textarea data-testid="partner-description-textarea">...</textarea>
  
  <!-- Form buttons -->
  <button data-testid="submit-partner-button">Submit</button>
  <button data-testid="cancel-partner-button">Cancel</button>
  
  <!-- Validation messages -->
  <span data-testid="partner-name-error">Required</span>
</form>
```

**Dialogs:**
```html
<p-dialog data-testid="delete-partner-dialog">
  <div data-testid="dialog-title">Delete Partner</div>
  <div data-testid="dialog-message">Are you sure?</div>
  <button data-testid="confirm-delete-button">Delete</button>
  <button data-testid="cancel-delete-button">Cancel</button>
</p-dialog>
```

---

## 📝 **Implementation Examples**

### **Example 1: Partner List Component**

**Before:**
```html
<div class="flex items-center justify-between mb-4">
  <div class="flex items-center gap-2">
    <i class="pi pi-briefcase text-2xl"></i>
    <h1 class="text-2xl font-bold">Partners</h1>
  </div>
  <div class="flex gap-2">
    <button (click)="createPartner()">New Partner</button>
    <button (click)="exportData()">Export</button>
  </div>
</div>

<app-listview [data]="partners"></app-listview>
```

**After:**
```html
<div 
  data-testid="partners-header"
  class="flex items-center justify-between mb-4">
  
  <div class="flex items-center gap-2">
    <i 
      data-testid="partners-icon"
      class="pi pi-briefcase text-2xl"></i>
    <h1 
      data-testid="partners-title"
      class="text-2xl font-bold">Partners</h1>
  </div>
  
  <div class="flex gap-2">
    <button 
      data-testid="new-partner-button"
      (click)="createPartner()">New Partner</button>
    <button 
      data-testid="export-button"
      (click)="exportData()">Export</button>
  </div>
</div>

<app-listview 
  data-testid="partners-listview"
  [data]="partners"></app-listview>
```

---

### **Example 2: Partner Detail Component**

**Before:**
```html
<div class="card p-4">
  <div class="flex justify-between items-center">
    <h2 class="text-xl font-bold">{{partner.name}}</h2>
    <div class="flex gap-2">
      <button (click)="editPartner()">Edit</button>
      <button (click)="deletePartner()">Delete</button>
    </div>
  </div>
  
  <div class="mt-4">
    <p><strong>Type:</strong> {{partner.type}}</p>
    <p><strong>Status:</strong> {{partner.status}}</p>
    <p><strong>Description:</strong> {{partner.description}}</p>
  </div>
  
  <div class="mt-6">
    <h3 class="text-lg font-semibold">Contacts</h3>
    <div *ngFor="let contact of partner.contacts">
      <p>{{contact.name}} - {{contact.email}}</p>
    </div>
  </div>
</div>
```

**After:**
```html
<div class="card p-4">
  <div 
    data-testid="partner-detail-header"
    class="flex justify-between items-center">
    
    <h2 
      data-testid="partner-title"
      class="text-xl font-bold">{{partner.name}}</h2>
    
    <div class="flex gap-2">
      <button 
        data-testid="edit-partner-button"
        (click)="editPartner()">Edit</button>
      <button 
        data-testid="delete-partner-button"
        (click)="deletePartner()">Delete</button>
    </div>
  </div>
  
  <div class="mt-4">
    <p data-testid="partner-type">
      <strong>Type:</strong> {{partner.type}}
    </p>
    <p data-testid="partner-status">
      <strong>Status:</strong> {{partner.status}}
    </p>
    <p data-testid="partner-description">
      <strong>Description:</strong> {{partner.description}}
    </p>
  </div>
  
  <div 
    data-testid="partner-contacts-section"
    class="mt-6">
    <h3 class="text-lg font-semibold">Contacts</h3>
    <div 
      *ngFor="let contact of partner.contacts"
      data-testid="partner-contact-item">
      <p>{{contact.name}} - {{contact.email}}</p>
    </div>
  </div>
</div>
```

---

### **Example 3: Create Partner Form**

**Before:**
```html
<p-dialog [(visible)]="showDialog" header="Create Partner">
  <form [formGroup]="partnerForm" (ngSubmit)="onSubmit()">
    <div class="field">
      <label>Name *</label>
      <input pInputText formControlName="name" />
      <small *ngIf="nameError" class="p-error">Name is required</small>
    </div>
    
    <div class="field">
      <label>Type *</label>
      <p-select 
        formControlName="type" 
        [options]="partnerTypes">
      </p-select>
    </div>
    
    <div class="field">
      <label>Description</label>
      <textarea pTextarea formControlName="description"></textarea>
    </div>
    
    <div class="flex justify-end gap-2">
      <button type="button" (click)="cancel()">Cancel</button>
      <button type="submit">Create Partner</button>
    </div>
  </form>
</p-dialog>
```

**After:**
```html
<p-dialog 
  data-testid="create-partner-dialog"
  [(visible)]="showDialog" 
  header="Create Partner">
  
  <form 
    data-testid="create-partner-form"
    [formGroup]="partnerForm" 
    (ngSubmit)="onSubmit()">
    
    <div class="field">
      <label>Name *</label>
      <input 
        data-testid="partner-name-input"
        pInputText 
        formControlName="name" />
      <small 
        data-testid="partner-name-error"
        *ngIf="nameError" 
        class="p-error">Name is required</small>
    </div>
    
    <div class="field">
      <label>Type *</label>
      <p-select 
        data-testid="partner-type-select"
        formControlName="type" 
        [options]="partnerTypes">
      </p-select>
    </div>
    
    <div class="field">
      <label>Description</label>
      <textarea 
        data-testid="partner-description-textarea"
        pTextarea 
        formControlName="description"></textarea>
    </div>
    
    <div class="flex justify-end gap-2">
      <button 
        data-testid="cancel-partner-button"
        type="button" 
        (click)="cancel()">Cancel</button>
      <button 
        data-testid="submit-partner-button"
        type="submit">Create Partner</button>
    </div>
  </form>
</p-dialog>
```

---

## 🔍 **Component-Specific Guidelines**

### **Partner Components**
- **List**: `partners-header`, `partners-listview`, `new-partner-button`
- **Detail**: `partner-detail-header`, `partner-title`, `partner-type`, `partner-status`
- **Form**: `create-partner-form`, `partner-name-input`, `partner-type-select`
- **Sections**: `partner-contacts-section`, `partner-interactions-section`
- **Items**: `partner-contact-item`, `partner-interaction-item`

### **Contact Components**
- **List**: `contacts-header`, `contacts-listview`, `new-contact-button`
- **Detail**: `contact-detail-header`, `contact-name`, `contact-email`, `contact-phone`
- **Form**: `create-contact-form`, `contact-name-input`, `contact-email-input`
- **Sections**: `contact-interactions-section`, `contact-documents-section`
- **Items**: `contact-interaction-item`, `contact-document-item`

### **Interaction Components**
- **List**: `interactions-header`, `interactions-listview`, `new-interaction-button`
- **Detail**: `interaction-detail-header`, `interaction-type`, `interaction-date`
- **Form**: `create-interaction-form`, `interaction-type-select`, `interaction-date-input`
- **Sections**: `interaction-participants-section`, `interaction-opportunities-section`
- **Items**: `interaction-participant-item`, `interaction-opportunity-item`

### **Opportunity Components**
- **List**: `opportunities-header`, `opportunities-listview`, `new-opportunity-button`
- **Detail**: `opportunity-detail-header`, `opportunity-title`, `opportunity-value`, `opportunity-stage`
- **Form**: `create-opportunity-form`, `opportunity-title-input`, `opportunity-value-input`
- **Sections**: `opportunity-partners-section`, `opportunity-budget-section`
- **Items**: `opportunity-partner-item`, `opportunity-contact-item`

---

## ✅ **Checklist for Developers**

When adding data-testid attributes to a component:

- [ ] Page header has `{entity}-header` or `{entity}-detail-header`
- [ ] Page title has `{entity}-title`
- [ ] Primary actions have `new-{entity}-button`, `edit-{entity}-button`, `delete-{entity}-button`
- [ ] List view has `{entity}-listview`
- [ ] Form container has `create-{entity}-form` or `edit-{entity}-form`
- [ ] Form inputs have `{entity}-{field}-input` (or `-select`, `-textarea`)
- [ ] Form submit has `submit-{entity}-button`
- [ ] Form cancel has `cancel-{entity}-button`
- [ ] Validation errors have `{entity}-{field}-error`
- [ ] Sections have `{entity}-{section-name}-section`
- [ ] List items have `{entity}-{item-type}-item`
- [ ] Dialog/modal has `{action}-{entity}-dialog`

---

## 🚫 **Common Mistakes to Avoid**

### **❌ Don't use IDs or classes in tests:**
```typescript
// BAD
page.locator('#partner-123')
page.locator('.partner-card')

// GOOD
page.locator('[data-testid="partner-detail-header"]')
```

### **❌ Don't use overly specific selectors:**
```typescript
// BAD
page.locator('div.container > div.card > h2.title')

// GOOD
page.locator('[data-testid="partner-title"]')
```

### **❌ Don't forget to add for dynamic content:**
```html
<!-- BAD -->
<div *ngFor="let partner of partners">
  {{partner.name}}
</div>

<!-- GOOD -->
<div 
  *ngFor="let partner of partners"
  data-testid="partner-item">
  {{partner.name}}
</div>
```

---

## 📚 **Resources**

- **Playwright Best Practices**: https://playwright.dev/docs/best-practices
- **Component Development Guidelines**: `.cursor/rules/component-development.mdc`
- **Existing Examples**: See `partners.component.html`, `contacts.component.html`

---

## 🆘 **Need Help?**

**Questions about:**
- **Naming conventions**: Review this guide's examples
- **Which elements to add**: Check the "DO Use" section
- **Existing patterns**: Look at `partners.component.html` or `contacts.component.html`
- **Testing**: See `contacts.spec.ts` for usage examples

---

**Document Owner:** Dev & QA Teams  
**Last Updated:** 2026-01-26  
**Status:** Active Reference
