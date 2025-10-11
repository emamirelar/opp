# Angular ClientApp Codebase Analysis & Restructuring Recommendations

**Date**: October 11, 2025  
**Project**: UNOPS Opportunity Plus  
**Analyzed Folder**: `UNOPS.PAO.ClientApp/`  
**Angular Version**: 19.0.5

---

## 📊 Executive Summary

This document provides a comprehensive analysis of the Angular ClientApp codebase structure and identifies critical organizational issues that impact maintainability, scalability, and developer productivity. The current structure lacks clear separation of concerns, has confusing folder hierarchies, and inconsistent organization patterns across features.

**Key Findings**:
- 🔴 **CRITICAL**: Missing test coverage across the entire application
- ❌ Dual "shared" folders causing confusion
- ❌ Page components mixed with shared components
- ❌ Unclear component hierarchy (3+ levels of "reusables")
- ❌ Static data files (tours) stored in code folders
- ❌ Inconsistent feature module organization
- ❌ Services scattered across multiple locations
- ❌ Inconsistent component implementation (inline vs. separate templates)

---

## 🔍 Current Structure Overview

```
UNOPS.PAO.ClientApp/src/app/
├── core/                      # ✅ Good: Singleton services
├── features/
│   ├── admin/                 # ⚠️ Flat structure, inconsistent
│   ├── ai/                    # ⚠️ Missing proper organization
│   ├── internal/              # ⚠️ Nearly empty, questionable purpose
│   ├── partnerships/          # ✅ Well-structured
│   ├── search/                # ⚠️ Minimal structure
│   └── shared/                # ❌ PROBLEM: Features shouldn't have "shared"
├── layouts/                   # ✅ Good concept
│   └── components/topbar/     # ⚠️ 20 files, needs sub-organization
├── shared/                    # ❌ PROBLEM: Too many responsibilities
│   ├── components/            # Reusable components ⚠️ Some inline templates
│   ├── pages/components/      # ❌ PROBLEM: Pages aren't "shared"
│   ├── reusables/components/  # ❌ PROBLEM: Redundant with components/
│   ├── reusables/widgets/     # ❌ PROBLEM: What's the difference?
│   ├── services/              # 22+ services, needs categorization
│   ├── themes/                # ❌ PROBLEM: Should be at app-level
│   └── tours/                 # ❌ PROBLEM: JSON data in code folder
```

**Additional Issues**:
- ⚠️ **Inconsistent component pattern**: Mix of inline templates vs separate `.html` files
- ⚠️ **Inconsistent styles**: Mix of inline styles vs separate `.scss` files

---

## 🚨 Critical Issues Identified

### 1. Confusing Dual "Shared" Folders

**Issue**: Two separate "shared" folders exist with unclear boundaries

```
app/shared/                    # Contains pages, components, services, models, themes, tours
app/features/shared/           # Contains feature-specific shared items
```

**Problems**:
- Developers don't know which "shared" to use
- Creates confusion about true "shared" vs feature-specific code
- Violates separation of concerns principle

**Impact**: 🔴 High - Slows development, increases cognitive load

---

### 2. Pages Mixed with Shared Components

**Issue**: Full-page components buried in `shared/pages/components/`

```
shared/pages/components/
├── login/                     # ❌ Should be in features/auth/
├── home/                      # ❌ Should be in features/home/
├── not-found/                 # ❌ Should be in features/static-pages/
├── access-denied/             # ❌ Should be in features/static-pages/
├── coming-soon/               # ❌ Should be in features/static-pages/
└── listview/                  # ❌ Should be its own feature or shared properly
```

**Problems**:
- Pages are NOT shared components - they're routed entry points
- Mixes routing concerns with component reusability
- Makes routing configuration harder to understand
- Violates feature-based organization

**Impact**: 🔴 High - Architectural confusion, hard to navigate

---

### 3. Unclear Component Hierarchy

**Issue**: Three different folders for "reusable" components with no clear distinction

```
shared/
├── components/                # Small reusable components (6 items)
├── reusables/components/      # Also reusable components (14 items)
└── reusables/widgets/         # More components (2 items)
```

**Problems**:
- No clear rule for what goes where
- "Reusables" is redundant - all shared components should be reusable
- "Widgets" vs "Components" distinction is unclear
- Forces developers to guess correct location

**Impact**: 🟡 Medium - Causes hesitation, inconsistent placement

---

### 4. Tours as Code

**Issue**: Tour JSON files stored in `shared/tours/`

```
shared/tours/
├── aiassistant-tour.json
├── contact-tour.json
├── partner-tour.json
└── ... (14 JSON files)
```

**Problems**:
- JSON data files don't belong in TypeScript code folders
- Harder to update by non-developers
- Not properly treated as assets
- Bloats the `shared` module

**Impact**: 🟢 Low - Works but architecturally wrong

**Fix**: Move to `assets/tours/` or `public/assets/tours/`

---

### 5. Inconsistent Feature Structure

**Issue**: Features have wildly different organization patterns

#### ✅ **Good Example**: `partnerships/`
```
partnerships/
├── contacts/
│   ├── components/
│   ├── models/
│   ├── services/
│   └── resolvers/
├── interactions/
│   ├── components/
│   ├── models/
│   └── services/
└── partners/
    ├── components/
    ├── models/
    ├── services/
    └── resolvers/
```

#### ❌ **Bad Example**: `admin/`
```
admin/
├── entity-manager/
│   ├── entity-manager.component.ts    # Flat structure
│   ├── entity-manager.component.html
│   └── entity-manager.component.scss
├── translation-workbench/
│   └── ... (same flat pattern)
└── user-management/
    ├── user-management.component.ts
    ├── user-management.component.html
    ├── user-management.component.scss
    └── user-management.service.ts      # Service directly here?
```

#### ❌ **Bad Example**: `ai/`
```
ai/
├── ai-content.component.ts            # Root component at wrong level
├── components/
│   └── ai-prompt/
├── models/
└── services/
```

#### ❌ **Bad Example**: `internal/`
```
internal/
├── components/
│   └── interaction/
│       └── detail/
│           └── interaction-detail.component.ts
└── internal-routing.module.ts         # Nearly empty module
```

**Problems**:
- No standard feature organization pattern
- Developers can't predict structure
- Harder to enforce code reviews
- Difficult to apply consistent linting rules

**Impact**: 🔴 High - Poor maintainability, team confusion

---

### 6. Services Scattered Everywhere

**Issue**: No clear guideline on where services belong

```
core/services/              (10 services)
├── auth.service.ts
├── configuration.service.ts
├── global-filter.service.ts
├── permission.service.ts
└── ...

shared/services/            (22+ services)
├── cached-data.service.ts
├── document.service.ts
├── feedback-dialog.service.ts
├── language.service.ts
├── logger.service.ts
├── notification.service.ts
├── tour.service.ts
├── user-profile.service.ts
├── workflow.service.ts
└── ... (many more)

features/*/services/        (scattered)
├── ai/services/
├── partnerships/*/services/
└── admin/user-management.service.ts  # Sometimes at component level?
```

**Problems**:
- Unclear distinction between core vs shared services
- Flat list of 22+ services is hard to navigate
- No categorization by purpose
- Difficult to find the right service

**Impact**: 🟡 Medium - Time wasted searching, duplicated services

---

### 7. Themes in Wrong Place

**Issue**: PrimeNG theme configuration in `shared/themes/`

```
shared/themes/
├── styles/
│   └── recordPage.scss
└── unops.preset.ts
```

**Problems**:
- Theme configuration is app-level, not "shared component"
- Should be alongside other styles
- Wrong abstraction level

**Impact**: 🟢 Low - Works but architecturally misplaced

**Fix**: Move to `src/styles/themes/` or `src/config/themes/`

---

### 8. Layouts Need Better Organization

**Issue**: Topbar component has 20 files with no sub-structure

```
layouts/components/topbar/
├── (20 files: 7 *.ts, 6 *.html, 5 *.scss, ...)
```

**Problems**:
- Too many files at one level
- Likely has sub-components that should be in folders
- Hard to distinguish what each file does
- Difficult to code review

**Impact**: 🟡 Medium - Harder to maintain topbar

---

### 9. Missing Test Coverage

**Issue**: Comprehensive lack of unit tests, integration tests, and end-to-end tests across the application

**Current State**:
```
✅ Test infrastructure: Jasmine + Karma configured
❌ Test coverage: Minimal to none
❌ Testing standards: Not defined
❌ CI/CD integration: Unknown
```

**Problems**:
- **Zero confidence in changes**: No safety net when refactoring
- **Production bugs**: Issues not caught before deployment
- **Difficult to maintain**: Can't verify if changes break existing functionality
- **Slower development**: Manual testing takes significantly longer
- **Technical debt**: Harder to refactor without tests
- **No regression prevention**: Same bugs can reappear
- **Poor code quality**: Untested code tends to be harder to test (tight coupling)
- **Documentation gap**: Tests serve as living documentation

**Impact**: 🔴 **CRITICAL** - This is arguably the most important issue to address

**Testing Stack Recommendation**:

```
Unit Tests:           Jasmine + Karma (already configured)
                      OR Jest (faster, better DX)
                      
E2E Tests:            Playwright (best headless support, cross-browser)
                      OR Cypress (good DX, visual testing)
                      
Component Tests:      Angular Testing Library (better practices)
                      
Code Coverage:        Istanbul (built-in with Karma/Jest)
                      Target: 80% minimum

Visual Regression:    Playwright Screenshots or Chromatic
                      (optional but recommended)
```

**Testing Standards to Implement**:

1. **Minimum Coverage Requirements**:
   - Services: 90% coverage
   - Components: 80% coverage  
   - Pipes/Directives: 95% coverage
   - Guards/Interceptors: 100% coverage
   - Overall: 80% coverage minimum

2. **Required Tests**:
   - ✅ Every service MUST have unit tests
   - ✅ Every component MUST have unit tests
   - ✅ Every pipe MUST have unit tests
   - ✅ Every guard MUST have unit tests
   - ✅ Critical user flows MUST have E2E tests

3. **Test File Structure**:
   ```
   component-name/
   ├── component-name.component.ts
   ├── component-name.component.html
   ├── component-name.component.scss
   └── component-name.component.spec.ts    ⬅️ REQUIRED
   
   service-name.service.ts
   service-name.service.spec.ts            ⬅️ REQUIRED
   ```

**Impact**: 🔴 **CRITICAL** - Foundation for maintainable, reliable application

---

### 10. Inconsistent Component Implementation Pattern

**Issue**: Components use mixed patterns for templates and styles - some inline, some in separate files

**Examples of Inconsistency**:

```typescript
// ❌ Bad: Inline template
@Component({
  selector: 'app-some-component',
  template: `
    <div>
      <h1>Inline HTML</h1>
      <p>This should be in a separate file</p>
    </div>
  `,
  styles: [`
    div { padding: 10px; }
    h1 { color: blue; }
  `]
})

// ✅ Good: Separate files
@Component({
  selector: 'app-another-component',
  templateUrl: './another-component.component.html',
  styleUrls: ['./another-component.component.scss']
})
```

**Problems**:
- **Inconsistent codebase**: Developers don't know which pattern to follow
- **Harder to maintain**: Inline templates are harder to read, edit, and format
- **No syntax highlighting**: Many IDEs don't highlight inline HTML/CSS properly
- **Difficult code reviews**: Reviewing HTML in TypeScript files is harder
- **Can't use CSS preprocessors effectively**: Inline styles lose SCSS/SASS benefits
- **Breaks separation of concerns**: Mixes presentation with logic
- **Harder to test**: Can't easily mock or stub templates
- **No template tooling**: Formatters, linters work better with separate files
- **Version control issues**: Template changes pollute component diffs

**Impact**: 🔴 High - Reduces code quality, maintainability, and developer experience

**Standard to Adopt**: **ALL components must have:**
```
component-name/
├── component-name.component.ts       # Component logic
├── component-name.component.html     # Template (always separate)
├── component-name.component.scss     # Styles (always separate, prefer SCSS)
└── component-name.component.spec.ts  # Unit tests
```

**Exceptions** (very rare, must be justified):
- Components with literally 1-2 lines of template AND no styling
- Example: Simple wrapper components or directives with minimal view

**Migration Action**:
1. Audit all components: `grep -r "template:" src/app --include="*.ts"`
2. Extract inline templates to `.html` files
3. Extract inline styles to `.scss` files
4. Update component decorators to use `templateUrl` and `styleUrls`
5. Add linting rule to prevent inline templates in new code

---

## ✅ Recommended Folder Structure

### Complete Recommended Structure

```
src/
├── app/
│   ├── app.component.ts
│   ├── app.config.ts
│   ├── app.module.ts
│   ├── app.routes.ts
│   │
│   ├── core/                          # Singleton services, guards, interceptors
│   │   ├── guards/
│   │   │   ├── auth.guard.ts
│   │   │   ├── auth.guard.spec.ts              # ⬅️ TEST
│   │   │   ├── admin.guard.ts
│   │   │   ├── admin.guard.spec.ts             # ⬅️ TEST
│   │   │   ├── role.guard.ts
│   │   │   ├── role.guard.spec.ts              # ⬅️ TEST
│   │   │   ├── route-permission.guard.ts
│   │   │   ├── route-permission.guard.spec.ts  # ⬅️ TEST
│   │   │   └── index.ts
│   │   │
│   │   ├── interceptors/
│   │   │   ├── auth.interceptor.ts
│   │   │   ├── auth.interceptor.spec.ts        # ⬅️ TEST
│   │   │   ├── server-error.interceptor.ts
│   │   │   └── server-error.interceptor.spec.ts # ⬅️ TEST
│   │   │
│   │   ├── services/
│   │   │   ├── auth/
│   │   │   │   ├── auth.service.ts
│   │   │   │   ├── auth.service.spec.ts         # ⬅️ TEST
│   │   │   │   ├── permission.service.ts
│   │   │   │   ├── permission.service.spec.ts   # ⬅️ TEST
│   │   │   │   ├── permission-utility.service.ts
│   │   │   │   ├── permission-utility.service.spec.ts # ⬅️ TEST
│   │   │   │   ├── role.service.ts
│   │   │   │   └── role.service.spec.ts         # ⬅️ TEST
│   │   │   ├── configuration/
│   │   │   │   ├── configuration.service.ts
│   │   │   │   └── configuration.service.spec.ts # ⬅️ TEST
│   │   │   ├── filters/
│   │   │   │   ├── global-filter.service.ts
│   │   │   │   ├── global-filter.service.spec.ts # ⬅️ TEST
│   │   │   │   ├── global-filters-dialog.service.ts
│   │   │   │   └── global-filters-dialog.service.spec.ts # ⬅️ TEST
│   │   │   └── organization/
│   │   │       ├── organization-hierarchy.service.ts
│   │   │       └── organization-hierarchy.service.spec.ts # ⬅️ TEST
│   │   │
│   │   ├── models/
│   │   │   └── organization-hierarchy.model.ts
│   │   │
│   │   └── core.module.ts             # Optional: for providing core services
│   │
│   ├── shared/                        # ONLY truly reusable items
│   │   │
│   │   ├── components/                # Reusable UI components
│   │   │   ├── forms/
│   │   │   │   ├── phone-input/
│   │   │   │   │   ├── phone-input.component.ts
│   │   │   │   │   ├── phone-input.component.spec.ts    # ⬅️ TEST
│   │   │   │   │   ├── phone-input.component.html
│   │   │   │   │   └── phone-input.component.scss
│   │   │   │   └── file-upload/
│   │   │   │       ├── file-upload.component.ts
│   │   │   │       └── file-upload.component.spec.ts    # ⬅️ TEST
│   │   │   │
│   │   │   ├── data-display/
│   │   │   │   ├── entity-tags/
│   │   │   │   │   ├── entity-tags.component.ts
│   │   │   │   │   ├── entity-tags.component.spec.ts    # ⬅️ TEST
│   │   │   │   │   └── README.md
│   │   │   │   ├── dashboard-card/
│   │   │   │   │   ├── dashboard-card.component.ts
│   │   │   │   │   ├── dashboard-card.component.spec.ts # ⬅️ TEST
│   │   │   │   │   ├── dashboard-card.models.ts
│   │   │   │   │   ├── index.ts
│   │   │   │   │   └── README.md
│   │   │   │   └── timeline/
│   │   │   │       ├── timeline.component.ts
│   │   │   │       ├── timeline.component.spec.ts       # ⬅️ TEST
│   │   │   │       ├── timeline.component.html
│   │   │   │       └── timeline.component.css
│   │   │   │
│   │   │   ├── feedback/
│   │   │   │   ├── iap-status/
│   │   │   │   │   ├── iap-status.component.ts
│   │   │   │   │   └── iap-status.component.spec.ts     # ⬅️ TEST
│   │   │   │   └── feedback-dialog/
│   │   │   │       ├── feedback-dialog.component.ts
│   │   │   │       ├── feedback-dialog.component.spec.ts # ⬅️ TEST
│   │   │   │       ├── feedback-dialog.component.html
│   │   │   │       └── feedback-dialog.component.css
│   │   │   │
│   │   │   ├── documents/
│   │   │   │   ├── document/
│   │   │   │   │   ├── document.component.ts
│   │   │   │   │   ├── document.component.spec.ts       # ⬅️ TEST
│   │   │   │   │   ├── document.component.html
│   │   │   │   │   └── document.component.scss
│   │   │   │   ├── document-list/
│   │   │   │   │   ├── document-list.component.ts
│   │   │   │   │   ├── document-list.component.spec.ts  # ⬅️ TEST
│   │   │   │   │   ├── document-list.component.html
│   │   │   │   │   └── document-list.component.scss
│   │   │   │   └── document-upload/
│   │   │   │       ├── document-upload.component.ts
│   │   │   │       ├── document-upload.component.spec.ts # ⬅️ TEST
│   │   │   │       ├── document-upload.component.html
│   │   │   │       └── document-upload.component.scss
│   │   │   │
│   │   │   ├── links/
│   │   │   │   ├── link/
│   │   │   │   │   └── ...
│   │   │   │   └── link-list/
│   │   │   │       └── ...
│   │   │   │
│   │   │   ├── media/
│   │   │   │   └── picture/
│   │   │   │       ├── picture.component.ts
│   │   │   │       ├── picture.component.html
│   │   │   │       └── picture-editor/
│   │   │   │           └── ...
│   │   │   │
│   │   │   ├── navigation/
│   │   │   │   ├── go-back/
│   │   │   │   │   └── go-back.component.ts
│   │   │   │   └── responsive-tabs/
│   │   │   │       ├── responsive-tabs.component.ts
│   │   │   │       ├── responsive-tabs.model.ts
│   │   │   │       └── index.ts
│   │   │   │
│   │   │   ├── layout/
│   │   │   │   ├── splitter/
│   │   │   │   │   ├── splitter.component.ts
│   │   │   │   │   ├── splitter.component.html
│   │   │   │   │   └── splitter.component.scss
│   │   │   │   └── loading-overlay/
│   │   │   │       └── loading-overlay.component.ts
│   │   │   │
│   │   │   ├── tours/
│   │   │   │   └── tour-control/
│   │   │   │       ├── tour-control.component.ts
│   │   │   │       └── tour-control.component.html
│   │   │   │
│   │   │   ├── analytics/
│   │   │   │   └── lookerstudio/
│   │   │   │       ├── lookerstudio.component.ts
│   │   │   │       ├── lookerstudio.component.html
│   │   │   │       └── lookerstudio.component.scss
│   │   │   │
│   │   │   └── workflows/
│   │   │       └── workflow/
│   │   │           ├── workflow.component.ts
│   │   │           ├── workflow.component.html
│   │   │           └── workflow.component.scss
│   │   │
│   │   ├── directives/
│   │   │   ├── has-permission.directive.ts
│   │   │   └── has-permission.directive.spec.ts  # ⬅️ TEST
│   │   │
│   │   ├── pipes/
│   │   │   ├── markdown.pipe.ts
│   │   │   └── markdown.pipe.spec.ts              # ⬅️ TEST
│   │   │
│   │   ├── models/
│   │   │   ├── api-responses.model.ts
│   │   │   ├── entity-tag.model.ts
│   │   │   ├── error.model.ts
│   │   │   ├── link.model.ts
│   │   │   ├── pagination-params.model.ts
│   │   │   ├── pagination-response.model.ts
│   │   │   ├── base-engagement.model.ts
│   │   │   ├── entity-status.enum.ts
│   │   │   ├── shared-types.ts
│   │   │   └── user.model.ts
│   │   │
│   │   ├── interfaces/
│   │   │   ├── document.interface.ts
│   │   │   ├── feedback.ts
│   │   │   ├── saved-filter.interface.ts
│   │   │   ├── tour.interface.ts
│   │   │   └── types.ts
│   │   │
│   │   ├── services/
│   │   │   ├── api/                   # API-related services
│   │   │   │   ├── base-engagement.service.ts
│   │   │   │   ├── base-engagement.service.spec.ts      # ⬅️ TEST
│   │   │   │   ├── document.service.ts
│   │   │   │   ├── document.service.spec.ts             # ⬅️ TEST
│   │   │   │   ├── link.service.ts
│   │   │   │   ├── link.service.spec.ts                 # ⬅️ TEST
│   │   │   │   ├── entity-configuration.service.ts
│   │   │   │   └── entity-configuration.service.spec.ts # ⬅️ TEST
│   │   │   │
│   │   │   ├── ui/                    # UI-related services
│   │   │   │   ├── notification.service.ts
│   │   │   │   ├── notification.service.spec.ts         # ⬅️ TEST
│   │   │   │   ├── feedback-dialog.service.ts
│   │   │   │   ├── feedback-dialog.service.spec.ts      # ⬅️ TEST
│   │   │   │   ├── tour.service.ts
│   │   │   │   ├── tour.service.spec.ts                 # ⬅️ TEST
│   │   │   │   ├── welcome-tour.service.ts
│   │   │   │   ├── welcome-tour.service.spec.ts         # ⬅️ TEST
│   │   │   │   ├── entity-panel.service.ts
│   │   │   │   └── entity-panel.service.spec.ts         # ⬅️ TEST
│   │   │   │
│   │   │   ├── utils/                 # Utility services
│   │   │   │   ├── logger.service.ts
│   │   │   │   ├── logger.service.spec.ts               # ⬅️ TEST
│   │   │   │   ├── error-handler.service.ts
│   │   │   │   ├── error-handler.service.spec.ts        # ⬅️ TEST
│   │   │   │   ├── language.service.ts
│   │   │   │   ├── language.service.spec.ts             # ⬅️ TEST
│   │   │   │   ├── search-parser.service.ts
│   │   │   │   ├── search-parser.service.spec.ts        # ⬅️ TEST
│   │   │   │   ├── cached-data.service.ts
│   │   │   │   ├── cached-data.service.spec.ts          # ⬅️ TEST
│   │   │   │   ├── fetch-stream.service.ts
│   │   │   │   ├── fetch-stream.service.spec.ts         # ⬅️ TEST
│   │   │   │   ├── page-context.service.ts
│   │   │   │   ├── page-context.service.spec.ts         # ⬅️ TEST
│   │   │   │   ├── component-resolver.service.ts
│   │   │   │   └── component-resolver.service.spec.ts   # ⬅️ TEST
│   │   │   │
│   │   │   ├── user/
│   │   │   │   ├── user-profile.service.ts
│   │   │   │   ├── user-profile.service.spec.ts         # ⬅️ TEST
│   │   │   │   ├── user-preference.service.ts
│   │   │   │   ├── user-preference.service.spec.ts      # ⬅️ TEST
│   │   │   │   ├── user-search.service.ts
│   │   │   │   └── user-search.service.spec.ts          # ⬅️ TEST
│   │   │   │
│   │   │   ├── domain/
│   │   │   │   ├── saved-filter.service.ts
│   │   │   │   ├── saved-filter.service.spec.ts         # ⬅️ TEST
│   │   │   │   ├── workflow.service.ts
│   │   │   │   ├── workflow.service.spec.ts             # ⬅️ TEST
│   │   │   │   ├── interaction-icon.service.ts
│   │   │   │   ├── interaction-icon.service.spec.ts     # ⬅️ TEST
│   │   │   │   ├── pagination-url.service.ts
│   │   │   │   └── pagination-url.service.spec.ts       # ⬅️ TEST
│   │   │   │
│   │   │   └── integration/           # 3rd party integrations
│   │   │       ├── drive-picker.service.ts
│   │   │       ├── drive-picker.service.spec.ts         # ⬅️ TEST
│   │   │       ├── export-google-sheet.service.ts
│   │   │       ├── export-google-sheet.service.spec.ts  # ⬅️ TEST
│   │   │       ├── import-google-sheet.service.ts
│   │   │       └── import-google-sheet.service.spec.ts  # ⬅️ TEST
│   │   │
│   │   ├── base-classes/              # Abstract base classes
│   │   │   ├── base-engagement-list.component.ts
│   │   │   ├── base-engagement-list.component.spec.ts  # ⬅️ TEST
│   │   │   ├── base-engagement-view.component.ts
│   │   │   ├── base-engagement-view.component.spec.ts  # ⬅️ TEST
│   │   │   ├── feature-base.component.ts
│   │   │   ├── feature-base.component.spec.ts          # ⬅️ TEST
│   │   │   ├── feature-base.component.html
│   │   │   └── feature-base.component.scss
│   │   │
│   │   └── shared.module.ts
│   │
│   ├── features/                      # Feature modules
│   │   │
│   │   ├── auth/                      # Authentication feature
│   │   │   ├── components/
│   │   │   │   ├── login/
│   │   │   │   │   ├── login.component.ts
│   │   │   │   │   ├── login.component.spec.ts          # ⬅️ TEST
│   │   │   │   │   ├── login.component.html
│   │   │   │   │   └── login.component.scss
│   │   │   │   ├── sign-up/
│   │   │   │   │   ├── sign-up.component.ts
│   │   │   │   │   ├── sign-up.component.spec.ts        # ⬅️ TEST
│   │   │   │   │   ├── sign-up.component.html
│   │   │   │   │   └── sign-up.component.scss
│   │   │   │   └── social-auth/
│   │   │   │       ├── social-auth.component.ts
│   │   │   │       ├── social-auth.component.spec.ts    # ⬅️ TEST
│   │   │   │       ├── social-auth.component.html
│   │   │   │       └── social-auth.component.css
│   │   │   ├── services/
│   │   │   │   └── (feature-specific auth services if needed + .spec.ts)
│   │   │   └── auth.routes.ts
│   │   │
│   │   ├── home/                      # Home/Dashboard feature
│   │   │   ├── components/
│   │   │   │   ├── home/
│   │   │   │   │   ├── home.component.ts
│   │   │   │   │   ├── home.component.spec.ts           # ⬅️ TEST
│   │   │   │   │   ├── home.component.html
│   │   │   │   │   └── home.component.css
│   │   │   │   └── home-dashboard/
│   │   │   │       ├── home-dashboard.component.ts
│   │   │   │       ├── home-dashboard.component.spec.ts # ⬅️ TEST
│   │   │   │       ├── home-dashboard.component.html
│   │   │   │       └── home-dashboard.component.scss
│   │   │   ├── services/
│   │   │   │   └── (dashboard services if needed + .spec.ts)
│   │   │   └── home.routes.ts
│   │   │
│   │   ├── partnerships/              # ✅ Already well-structured!
│   │   │   ├── contacts/
│   │   │   │   ├── components/
│   │   │   │   │   └── contact/
│   │   │   │   │       ├── list/
│   │   │   │   │       ├── view/
│   │   │   │   │       ├── tabs/
│   │   │   │   │       └── edit-dialog/
│   │   │   │   ├── models/
│   │   │   │   │   ├── contact.model.ts
│   │   │   │   │   ├── contact-view.model.ts
│   │   │   │   │   └── contact-filter-params.model.ts
│   │   │   │   ├── services/
│   │   │   │   │   ├── contact.service.ts
│   │   │   │   │   ├── contact.service.spec.ts          # ⬅️ TEST
│   │   │   │   │   ├── contact-export.service.ts
│   │   │   │   │   └── contact-export.service.spec.ts   # ⬅️ TEST
│   │   │   │   └── resolvers/
│   │   │   │       ├── contact-data.resolver.ts
│   │   │   │       └── contact-data.resolver.spec.ts    # ⬅️ TEST
│   │   │   │
│   │   │   ├── interactions/
│   │   │   │   ├── components/
│   │   │   │   │   └── interaction/
│   │   │   │   │       ├── list/
│   │   │   │   │       ├── detail/
│   │   │   │   │       ├── modal/
│   │   │   │   │       └── preview/
│   │   │   │   ├── models/
│   │   │   │   │   ├── interaction.model.ts
│   │   │   │   │   ├── interaction-type.enum.ts
│   │   │   │   │   └── interaction-filter-params.model.ts
│   │   │   │   └── services/
│   │   │   │       ├── interaction.service.ts
│   │   │   │       └── interaction.service.spec.ts      # ⬅️ TEST
│   │   │   │
│   │   │   ├── partners/
│   │   │   │   ├── components/
│   │   │   │   │   ├── partner/
│   │   │   │   │   │   ├── partner.component.ts
│   │   │   │   │   │   ├── view/
│   │   │   │   │   │   ├── tabs/
│   │   │   │   │   │   ├── data/
│   │   │   │   │   │   ├── contacts/
│   │   │   │   │   │   ├── edit-dialog/
│   │   │   │   │   │   ├── funding-agreements/
│   │   │   │   │   │   └── approval-dialog/
│   │   │   │   │   ├── partner-tree/
│   │   │   │   │   │   ├── partner-tree.component.ts
│   │   │   │   │   │   ├── partner-tree.component.html
│   │   │   │   │   │   ├── item/
│   │   │   │   │   │   └── view/
│   │   │   │   │   └── partner-tree-page/
│   │   │   │   │       └── partner-tree-page.component.ts
│   │   │   │   ├── models/
│   │   │   │   │   ├── partner.model.ts
│   │   │   │   │   ├── partner-tree.model.ts
│   │   │   │   │   ├── partner-category-group.model.ts
│   │   │   │   │   └── organization-unit-relationship.model.ts
│   │   │   │   ├── services/
│   │   │   │   │   ├── partner.service.ts
│   │   │   │   │   ├── partner.service.spec.ts          # ⬅️ TEST
│   │   │   │   │   ├── partner-tree.service.ts
│   │   │   │   │   └── partner-tree.service.spec.ts     # ⬅️ TEST
│   │   │   │   └── resolvers/
│   │   │   │       ├── partner-data.resolver.ts
│   │   │   │       ├── partner-data.resolver.spec.ts    # ⬅️ TEST
│   │   │   │       ├── partner-tree-data.resolver.ts
│   │   │   │       └── partner-tree-data.resolver.spec.ts # ⬅️ TEST
│   │   │   │
│   │   │   └── partnerships.routes.ts
│   │   │
│   │   ├── admin/
│   │   │   ├── user-management/
│   │   │   │   ├── components/
│   │   │   │   │   ├── user-management.component.ts
│   │   │   │   │   ├── user-management.component.spec.ts  # ⬅️ TEST
│   │   │   │   │   ├── user-management.component.html
│   │   │   │   │   └── user-management.component.scss
│   │   │   │   ├── services/
│   │   │   │   │   ├── user-management.service.ts
│   │   │   │   │   └── user-management.service.spec.ts    # ⬅️ TEST
│   │   │   │   └── models/
│   │   │   │       └── (if needed)
│   │   │   │
│   │   │   ├── entity-manager/
│   │   │   │   ├── components/
│   │   │   │   │   ├── entity-manager.component.ts
│   │   │   │   │   ├── entity-manager.component.spec.ts   # ⬅️ TEST
│   │   │   │   │   ├── entity-manager.component.html
│   │   │   │   │   └── entity-manager.component.scss
│   │   │   │   ├── services/
│   │   │   │   │   └── (if needed + .spec.ts)
│   │   │   │   └── models/
│   │   │   │       └── (if needed)
│   │   │   │
│   │   │   ├── translation-workbench/
│   │   │   │   ├── components/
│   │   │   │   │   ├── translation-workbench.component.ts
│   │   │   │   │   ├── translation-workbench.component.spec.ts # ⬅️ TEST
│   │   │   │   │   ├── translation-workbench.component.html
│   │   │   │   │   └── translation-workbench.component.scss
│   │   │   │   ├── services/
│   │   │   │   │   └── (if needed + .spec.ts)
│   │   │   │   └── models/
│   │   │   │       └── (if needed)
│   │   │   │
│   │   │   └── admin.routes.ts
│   │   │
│   │   ├── ai/
│   │   │   ├── components/
│   │   │   │   ├── ai-content/
│   │   │   │   │   ├── ai-content.component.ts
│   │   │   │   │   └── ai-content.component.spec.ts     # ⬅️ TEST
│   │   │   │   ├── ai-prompt/
│   │   │   │   │   ├── ai-prompt.component.ts
│   │   │   │   │   ├── ai-prompt.component.spec.ts      # ⬅️ TEST
│   │   │   │   │   ├── ai-prompt.component.html
│   │   │   │   │   └── ai-prompt.component.scss
│   │   │   │   ├── ai-panel/           # Move from shared
│   │   │   │   │   ├── ai-panel.component.ts
│   │   │   │   │   ├── ai-panel.component.spec.ts       # ⬅️ TEST
│   │   │   │   │   └── ai-panel.component.html
│   │   │   │   └── ai-transcribe/      # Move from shared
│   │   │   │       ├── ai-transcribe.component.ts
│   │   │   │       └── ai-transcribe.component.spec.ts  # ⬅️ TEST
│   │   │   │
│   │   │   ├── widgets/                # AI Assistant widget
│   │   │   │   └── ai-assistant/
│   │   │   │       ├── (multiple components + their .spec.ts files)
│   │   │   │       └── ...
│   │   │   │
│   │   │   ├── models/
│   │   │   │   ├── ai-assistant.model.ts
│   │   │   │   └── gemini.model.ts
│   │   │   │
│   │   │   ├── services/
│   │   │   │   ├── ai-assistant.service.ts
│   │   │   │   ├── ai-assistant.service.spec.ts         # ⬅️ TEST
│   │   │   │   ├── ai-prompt.service.ts
│   │   │   │   ├── ai-prompt.service.spec.ts            # ⬅️ TEST
│   │   │   │   ├── gemini.service.ts
│   │   │   │   └── gemini.service.spec.ts               # ⬅️ TEST
│   │   │   │
│   │   │   └── ai.routes.ts
│   │   │
│   │   ├── search/
│   │   │   ├── components/
│   │   │   │   ├── search-result/
│   │   │   │   │   ├── search-result.component.ts
│   │   │   │   │   ├── search-result.component.spec.ts  # ⬅️ TEST
│   │   │   │   │   └── search-result.component.html
│   │   │   │   └── advanced-search/    # Move from shared if search-specific
│   │   │   │       └── (components + .spec.ts files)
│   │   │   │
│   │   │   ├── services/
│   │   │   │   ├── search.service.ts   # If needed
│   │   │   │   └── search.service.spec.ts               # ⬅️ TEST
│   │   │   │
│   │   │   └── search.routes.ts
│   │   │
│   │   ├── list-view/                  # Generic list view feature
│   │   │   ├── components/
│   │   │   │   ├── list-view/
│   │   │   │   │   ├── listview.component.ts
│   │   │   │   │   ├── listview.component.spec.ts       # ⬅️ TEST
│   │   │   │   │   ├── listview.component.html
│   │   │   │   │   └── listview.model.ts
│   │   │   │   ├── list-view-card/
│   │   │   │   │   ├── listview-card.component.ts
│   │   │   │   │   ├── listview-card.component.spec.ts  # ⬅️ TEST
│   │   │   │   │   └── listview-card.component.html
│   │   │   │   └── advanced-search/
│   │   │   │       ├── listview-advanced-search.component.ts
│   │   │   │       ├── listview-advanced-search.component.spec.ts # ⬅️ TEST
│   │   │   │       ├── listview-advanced-search.component.html
│   │   │   │       └── saved-filter/
│   │   │   │           └── (components + .spec.ts files)
│   │   │   │
│   │   │   └── services/
│   │   │       ├── listview-export.service.ts
│   │   │       └── listview-export.service.spec.ts      # ⬅️ TEST
│   │   │
│   │   ├── import-export/              # Import/Export functionality
│   │   │   ├── components/
│   │   │   │   ├── import-dialog/
│   │   │   │   │   ├── import-dialog.component.ts
│   │   │   │   │   ├── import-dialog.component.spec.ts  # ⬅️ TEST
│   │   │   │   │   ├── import-dialog.component.html
│   │   │   │   │   ├── footer/
│   │   │   │   │   │   └── (components + .spec.ts files)
│   │   │   │   │   └── manual-entry/
│   │   │   │   │       └── (components + .spec.ts files)
│   │   │   │   ├── duplicate-indicator/
│   │   │   │   │   └── (components + .spec.ts files)
│   │   │   │   └── duplicate-summary/
│   │   │   │       └── (components + .spec.ts files)
│   │   │   │
│   │   │   └── services/
│   │   │       ├── import.service.ts
│   │   │       ├── import.service.spec.ts               # ⬅️ TEST
│   │   │       ├── import-dialog.service.ts
│   │   │       ├── import-dialog.service.spec.ts        # ⬅️ TEST
│   │   │       ├── import-google-sheet.service.ts
│   │   │       ├── import-google-sheet.service.spec.ts  # ⬅️ TEST
│   │   │       ├── export-google-sheet.service.ts
│   │   │       └── export-google-sheet.service.spec.ts  # ⬅️ TEST
│   │   │
│   │   └── static-pages/               # Static/system pages
│   │       └── components/
│   │           ├── not-found/
│   │           │   ├── not-found.component.ts
│   │           │   ├── not-found.component.spec.ts      # ⬅️ TEST
│   │           │   ├── not-found.component.html
│   │           │   └── not-found.component.scss
│   │           ├── access-denied/
│   │           │   ├── access-denied.component.ts
│   │           │   ├── access-denied.component.spec.ts  # ⬅️ TEST
│   │           │   ├── access-denied.component.html
│   │           │   └── access-denied.component.scss
│   │           └── coming-soon/
│   │               ├── coming-soon.component.ts
│   │               ├── coming-soon.component.spec.ts    # ⬅️ TEST
│   │               └── coming-soon.component.html
│   │
│   └── layouts/                        # Layout components
│       ├── components/
│       │   ├── main-layout/
│       │   │   ├── layout.component.ts
│       │   │   ├── layout.component.spec.ts             # ⬅️ TEST
│       │   │   ├── layout.component.html
│       │   │   └── layout.component.scss
│       │   │
│       │   ├── topbar/
│       │   │   ├── topbar.component.ts
│       │   │   ├── topbar.component.spec.ts             # ⬅️ TEST
│       │   │   ├── topbar.component.html
│       │   │   ├── topbar.component.scss
│       │   │   ├── components/         # Sub-components of topbar
│       │   │   │   ├── notifications/
│       │   │   │   │   └── (components + .spec.ts files)
│       │   │   │   ├── user-menu/
│       │   │   │   │   └── (components + .spec.ts files)
│       │   │   │   ├── global-search/
│       │   │   │   │   └── (components + .spec.ts files)
│       │   │   │   ├── breadcrumb/
│       │   │   │   │   └── (components + .spec.ts files)
│       │   │   │   └── filters/
│       │   │   │       └── (components + .spec.ts files)
│       │   │   └── models/
│       │   │       └── (if needed)
│       │   │
│       │   ├── sidebar/
│       │   │   ├── sidebar.component.ts
│       │   │   ├── sidebar.component.spec.ts            # ⬅️ TEST
│       │   │   ├── sidebar.component.html
│       │   │   └── sidebar.component.scss
│       │   │
│       │   ├── menu/
│       │   │   ├── menu.component.ts
│       │   │   ├── menu.component.spec.ts               # ⬅️ TEST
│       │   │   ├── menu.component.html
│       │   │   └── menu.component.scss
│       │   │
│       │   ├── footer/
│       │   │   ├── footer.component.ts
│       │   │   ├── footer.component.spec.ts             # ⬅️ TEST
│       │   │   ├── footer.component.html
│       │   │   └── footer.component.scss
│       │   │
│       │   └── profile-dialog/
│       │       ├── profile-dialog.component.ts
│       │       └── profile-dialog.component.spec.ts     # ⬅️ TEST
│       │
│       ├── services/
│       │   ├── layout.service.ts
│       │   └── layout.service.spec.ts                   # ⬅️ TEST
│       │
│       └── layouts.module.ts
│
├── assets/                             # Static assets (public in new Angular)
│   ├── i18n/
│   │   ├── en.json
│   │   ├── fr.json
│   │   ├── pt.json
│   │   └── span.json
│   │
│   ├── tours/                          # ⬅️ Move tour JSONs here
│   │   ├── aiassistant-tour.json
│   │   ├── contact-tour.json
│   │   ├── partner-tour.json
│   │   ├── interaction-tour.json
│   │   └── tour-registry.json
│   │
│   ├── images/
│   │   └── ...
│   │
│   └── ...
│
├── tests/                              # E2E and test utilities
│   ├── e2e/                            # Playwright E2E tests
│   │   ├── auth.spec.ts                # ⬅️ E2E TEST
│   │   ├── contact-management.spec.ts  # ⬅️ E2E TEST
│   │   ├── partner-management.spec.ts  # ⬅️ E2E TEST
│   │   ├── interaction-management.spec.ts # ⬅️ E2E TEST
│   │   └── admin.spec.ts               # ⬅️ E2E TEST
│   ├── fixtures/                       # Test data
│   │   ├── contact.fixtures.ts
│   │   ├── partner.fixtures.ts
│   │   └── user.fixtures.ts
│   └── helpers/                        # Test utilities
│       └── test-utils.ts
│
├── styles/                             # Global styles
│   ├── themes/                         # ⬅️ Move from shared
│   │   ├── unops.preset.ts
│   │   └── styles/
│   │       └── recordPage.scss
│   │
│   ├── primeng-unops-theme.scss
│   ├── unops-design-tokens.scss
│   ├── unops-design-tokens.css
│   ├── unops-utilities.scss
│   └── README.md
│
└── environments/                       # Environment configs (if using)
    ├── environment.ts
    └── environment.prod.ts
```

**Note**: Every `.ts` file (component, service, pipe, directive, guard, interceptor) MUST have a corresponding `.spec.ts` test file in the same directory. The `# ⬅️ TEST` markers above indicate where test files are required.

---

## 📋 Migration Plan

### Phase 1: Foundation Cleanup (Low Risk)

#### 1.1 Move Static Assets
```bash
# Move tour JSONs to assets
mv src/app/shared/tours/* public/assets/tours/

# Update tour service imports
# Update in: src/app/shared/services/tour.service.ts
# Change: import from '../tours/...'
# To:     Load from 'assets/tours/...' via HttpClient
```

#### 1.2 Move Theme Configuration
```bash
# Move themes to styles
mv src/app/shared/themes/* src/styles/themes/

# Update imports in components and angular.json
```

#### 1.3 Organize Core Services
```bash
# Create subcategories in core/services
mkdir -p src/app/core/services/{auth,configuration,filters,organization}

# Move services into appropriate folders
mv src/app/core/services/auth.service.ts src/app/core/services/auth/
mv src/app/core/services/permission*.ts src/app/core/services/auth/
mv src/app/core/services/role.service.ts src/app/core/services/auth/
# ... etc
```

---

### Phase 2: Shared Module Reorganization (Medium Risk)

#### 2.1 Organize Shared Components
```bash
# Create categorical folders
mkdir -p src/app/shared/components/{forms,data-display,feedback,documents,links,media,navigation,layout,tours,analytics,workflows}

# Move components to appropriate categories
mv src/app/shared/components/phone-input src/app/shared/components/forms/
mv src/app/shared/components/entity-tags src/app/shared/components/data-display/
# ... etc
```

#### 2.2 Consolidate Reusables
```bash
# Move reusables/components/* to shared/components/
mv src/app/shared/reusables/components/document src/app/shared/components/documents/
mv src/app/shared/reusables/components/link src/app/shared/components/links/
# ... etc

# Move reusables/widgets/* to appropriate feature or shared location
mv src/app/shared/reusables/widgets/ai-assistant src/app/features/ai/widgets/

# Remove empty reusables folder
rm -rf src/app/shared/reusables
```

#### 2.3 Organize Shared Services
```bash
# Create service categories
mkdir -p src/app/shared/services/{api,ui,utils,user,domain,integration}

# Move services into categories
mv src/app/shared/services/document.service.ts src/app/shared/services/api/
mv src/app/shared/services/link.service.ts src/app/shared/services/api/
mv src/app/shared/services/notification.service.ts src/app/shared/services/ui/
mv src/app/shared/services/tour.service.ts src/app/shared/services/ui/
mv src/app/shared/services/logger.service.ts src/app/shared/services/utils/
mv src/app/shared/services/user-profile.service.ts src/app/shared/services/user/
# ... etc
```

#### 2.4 Create Base Classes Folder
```bash
# Create base-classes folder
mkdir -p src/app/shared/base-classes

# Move base components
mv src/app/features/shared/base-engagement/* src/app/shared/base-classes/
mv src/app/shared/reusables/feature-base/* src/app/shared/base-classes/
```

---

### Phase 3: Feature Reorganization (Higher Risk)

#### 3.1 Create New Feature Modules
```bash
# Create auth feature
mkdir -p src/app/features/auth/components
mv src/app/shared/pages/components/login src/app/features/auth/components/
mv src/app/shared/pages/components/sign-up src/app/features/auth/components/login/
mv src/app/shared/pages/components/socialAuth src/app/features/auth/components/

# Create home feature
mkdir -p src/app/features/home/components
mv src/app/shared/pages/components/home src/app/features/home/components/

# Create static-pages feature
mkdir -p src/app/features/static-pages/components
mv src/app/shared/pages/components/not-found src/app/features/static-pages/components/
mv src/app/shared/pages/components/access-denied src/app/features/static-pages/components/
mv src/app/shared/pages/components/coming-soon src/app/features/static-pages/components/

# Create list-view feature
mkdir -p src/app/features/list-view/components
mv src/app/shared/pages/components/listview src/app/features/list-view/components/

# Create import-export feature
mkdir -p src/app/features/import-export/{components,services}
mv src/app/shared/reusables/components/import src/app/features/import-export/components/
mv src/app/shared/reusables/components/export src/app/features/import-export/services/
```

#### 3.2 Reorganize Admin Feature
```bash
# Create proper structure for each admin sub-feature
mkdir -p src/app/features/admin/{user-management,entity-manager,translation-workbench}/{components,services,models}

# Move components into components folder
mv src/app/features/admin/user-management/*.component.* src/app/features/admin/user-management/components/
mv src/app/features/admin/user-management/*.service.ts src/app/features/admin/user-management/services/

# Repeat for entity-manager and translation-workbench
```

#### 3.3 Reorganize AI Feature
```bash
# Create proper structure
mkdir -p src/app/features/ai/{components,widgets,models,services}

# Move root component to components
mv src/app/features/ai/ai-content.component.ts src/app/features/ai/components/ai-content/

# Move shared AI components from shared
mv src/app/shared/reusables/components/ai-panel src/app/features/ai/components/
mv src/app/shared/reusables/components/ai-transcribe src/app/features/ai/components/

# Move AI widgets
mv src/app/shared/reusables/widgets/ai-assistant src/app/features/ai/widgets/
```

#### 3.4 Remove features/shared Folder
```bash
# After moving all content from features/shared, remove it
rm -rf src/app/features/shared
```

---

### Phase 4: Layout Reorganization

#### 4.1 Organize Topbar Components
```bash
# Create sub-components folder in topbar
mkdir -p src/app/layouts/components/topbar/components/{notifications,user-menu,global-search,breadcrumb,filters}

# Analyze and move topbar's 20 files into appropriate sub-components
# This requires examining each file to understand its purpose
```

---

### Phase 5: Update Imports and Routes

#### 5.1 Update Path Mappings
```typescript
// tsconfig.json - Update path aliases
{
  "compilerOptions": {
    "paths": {
      "@core/*": ["src/app/core/*"],
      "@shared/*": ["src/app/shared/*"],
      "@features/*": ["src/app/features/*"],
      "@layouts/*": ["src/app/layouts/*"],
      "@auth/*": ["src/app/features/auth/*"],
      "@home/*": ["src/app/features/home/*"],
      "@partnerships/*": ["src/app/features/partnerships/*"],
      "@admin/*": ["src/app/features/admin/*"],
      "@ai/*": ["src/app/features/ai/*"],
      "@search/*": ["src/app/features/search/*"],
      "@assets/*": ["src/assets/*"],
      "@styles/*": ["src/styles/*"]
    }
  }
}
```

#### 5.2 Update Route Imports
```typescript
// app.routes.ts - Update all imports to new locations
import { LoginComponent } from '@auth/components/login/login.component';
import { HomeComponent } from '@home/components/home/home.component';
import { NotFoundComponent } from '@features/static-pages/components/not-found/not-found.component';
// ... etc
```

#### 5.3 Update Service Imports
```typescript
// Update all component imports of shared services
// Old: import { LoggerService } from '@shared/services/logger.service';
// New: import { LoggerService } from '@shared/services/utils/logger.service';

// Or create barrel exports
// shared/services/index.ts
export * from './api';
export * from './ui';
export * from './utils';
export * from './user';
export * from './domain';
export * from './integration';

// Then import as:
// import { LoggerService } from '@shared/services';
```

---

## 🎯 Key Architectural Principles

### 1. Core Module
**Purpose**: Singleton services that are used app-wide and should be loaded only once.

**What Belongs Here**:
- ✅ Authentication services
- ✅ Authorization services
- ✅ Configuration services
- ✅ Global state management
- ✅ HTTP interceptors
- ✅ Route guards
- ✅ App-wide directives (used everywhere)

**What Doesn't Belong**:
- ❌ UI components (even if used everywhere)
- ❌ Business logic services (those go in features)
- ❌ Utility functions (those go in shared)

---

### 2. Shared Module
**Purpose**: Code that is reused across **3 or more** feature modules.

**What Belongs Here**:
- ✅ Reusable UI components (buttons, inputs, cards, etc.)
- ✅ Reusable pipes
- ✅ Reusable directives
- ✅ Common models/interfaces used across features
- ✅ Utility services (logging, error handling, etc.)
- ✅ Base classes for inheritance

**What Doesn't Belong**:
- ❌ Pages/routed components
- ❌ Feature-specific logic
- ❌ Services used by only one feature
- ❌ Business domain models (those go in features)

**Rule of Thumb**: If you're not sure if something is "shared enough", put it in a feature first. Move it to shared only when a second feature needs it.

---

### 3. Features Module
**Purpose**: Self-contained business domains with their own logic, components, and services.

**What Belongs Here**:
- ✅ Routed/page components
- ✅ Feature-specific components
- ✅ Feature-specific services
- ✅ Domain models
- ✅ Route resolvers
- ✅ Feature-specific guards
- ✅ Feature routes configuration

**Structure Pattern** (for each feature):
```
feature-name/
├── components/           # All components for this feature
│   ├── feature-root/    # Main/entry component
│   └── sub-feature/     # Sub-components
├── models/              # Domain models, interfaces, enums
├── services/            # Business logic services
├── resolvers/           # Route data resolvers
├── guards/              # Feature-specific guards (if any)
└── feature-name.routes.ts
```

---

### 4. Layouts Module
**Purpose**: Shell components that structure the application layout.

**What Belongs Here**:
- ✅ Main layout component
- ✅ Header/topbar
- ✅ Sidebar/navigation
- ✅ Footer
- ✅ Layout-related services (responsive layout, etc.)

**What Doesn't Belong**:
- ❌ Business logic
- ❌ Feature components
- ❌ Reusable UI components (those go in shared)

---

### 5. Assets Folder
**Purpose**: Static files that are served as-is.

**What Belongs Here**:
- ✅ Images
- ✅ Fonts
- ✅ i18n translation files
- ✅ JSON configuration/data files (like tours)
- ✅ Static documents

**What Doesn't Belong**:
- ❌ TypeScript files
- ❌ SCSS files (except design tokens)
- ❌ Components

---

## 📊 Decision Matrix: Where Does This Code Go?

| If Your Code Is... | Put It In... | Example |
|-------------------|--------------|---------|
| A singleton service used app-wide | `core/services/` | AuthService, ConfigService |
| An HTTP interceptor | `core/interceptors/` | AuthInterceptor |
| A route guard used everywhere | `core/guards/` | AuthGuard |
| A reusable UI component (3+ features) | `shared/components/` | PhoneInput, EntityTags |
| A reusable pipe or directive | `shared/pipes/` or `shared/directives/` | MarkdownPipe |
| A utility service (3+ features) | `shared/services/utils/` | LoggerService |
| A page/routed component | `features/{feature}/components/` | LoginComponent, HomeComponent |
| Feature-specific business logic | `features/{feature}/services/` | ContactService |
| Domain models for a feature | `features/{feature}/models/` | Contact, Partner |
| Application shell/chrome | `layouts/components/` | TopBar, SideBar |
| JSON data/configuration | `assets/` or `public/assets/` | tour files, i18n |
| Theme/styling configuration | `styles/themes/` | PrimeNG presets |
| Error page (404, 403, etc.) | `features/static-pages/` | NotFoundComponent |

---

## ✅ Validation Checklist

After reorganization, verify:

### Architecture
- [ ] **No "shared" inside features**: `features/shared/` should not exist
- [ ] **No pages in shared**: Routed components are in feature modules
- [ ] **Organized services**: Services are categorized, not in flat list
- [ ] **Consistent feature structure**: All features follow same pattern (components, models, services)
- [ ] **Consistent component pattern**: All components use separate .html, .scss, and .ts files
- [ ] **No inline templates**: Search for `template:` returns only justified exceptions
- [ ] **No inline styles**: Search for `styles:` returns only justified exceptions
- [ ] **Assets are static**: No TypeScript code in assets folder
- [ ] **Themes at app level**: Theme configuration is in styles, not shared
- [ ] **Clear component categories**: Shared components are grouped by purpose
- [ ] **Barrel exports**: Major folders have index.ts for clean imports
- [ ] **Path aliases work**: TypeScript paths in tsconfig.json are updated

### Testing
- [ ] **All components have tests**: Every .component.ts has .component.spec.ts
- [ ] **All services have tests**: Every .service.ts has .service.spec.ts
- [ ] **All pipes have tests**: Every .pipe.ts has .pipe.spec.ts
- [ ] **All guards have tests**: Every .guard.ts has .guard.spec.ts
- [ ] **All interceptors have tests**: Every .interceptor.ts has .interceptor.spec.ts
- [ ] **Coverage meets minimum**: Overall coverage ≥ 80%
- [ ] **Coverage enforced**: karma.conf.js or jest.config has coverage thresholds
- [ ] **E2E tests exist**: Critical user flows have Playwright tests
- [ ] **E2E configured for headless**: Playwright runs in headless mode
- [ ] **Tests run in CI/CD**: GitHub Actions or similar runs tests automatically

### Build & Run
- [ ] **App builds**: `ng build` succeeds
- [ ] **App runs**: `ng serve` works without errors
- [ ] **Tests pass**: `npm test` succeeds with no failures
- [ ] **E2E tests pass**: `npx playwright test` succeeds
- [ ] **Coverage report generated**: Coverage report available in ./coverage/
- [ ] **Lint passes**: `ng lint` succeeds

---

## 🚀 Quick Wins (Implement First)

These changes have **low risk** and **high impact**:

### 1. Move Tour JSONs to Assets
**Impact**: ⭐⭐⭐⭐  
**Risk**: ⭐  
**Effort**: 15 minutes

```bash
mv src/app/shared/tours/*.json public/assets/tours/
```

Update `tour.service.ts` to load from assets via HTTP.

---

### 2. Move Themes to Styles
**Impact**: ⭐⭐⭐  
**Risk**: ⭐  
**Effort**: 10 minutes

```bash
mv src/app/shared/themes src/styles/
```

Update imports in components and angular.json.

---

### 3. Organize Core Services
**Impact**: ⭐⭐⭐⭐  
**Risk**: ⭐⭐  
**Effort**: 30 minutes

Create subcategories and move services. Add barrel exports.

---

### 4. Create Feature Barrel Exports
**Impact**: ⭐⭐⭐⭐  
**Risk**: ⭐  
**Effort**: 20 minutes

```typescript
// features/partnerships/contacts/index.ts
export * from './components/contact/list/contact-list.component';
export * from './services/contact.service';
export * from './models/contact.model';
```

Makes imports cleaner throughout the app.

---

### 5. Rename Confusing Folders
**Impact**: ⭐⭐⭐⭐⭐  
**Risk**: ⭐  
**Effort**: 5 minutes

```bash
# Add README files explaining purpose
echo "# Pages\nFull-page components with routes" > src/app/shared/pages/README.md
```

Or better: Start moving pages out to features (higher effort but correct solution).

---

### 6. Audit and Fix Component Templates
**Impact**: ⭐⭐⭐⭐⭐  
**Risk**: ⭐⭐  
**Effort**: 1-2 hours (one-time audit), ongoing enforcement

```bash
# Find components with inline templates
grep -r "template:" src/app --include="*.ts" | wc -l

# Find components with inline styles
grep -r "styles:" src/app --include="*.ts" | wc -l
```

**Actions**:
1. Create list of all components with inline templates
2. Extract templates to `.html` files (can be automated with script)
3. Extract styles to `.scss` files
4. Add ESLint rule to prevent future inline templates:

```json
// .eslintrc.json or eslint.config.js
{
  "rules": {
    "@angular-eslint/component-class-suffix": "error",
    "@angular-eslint/use-component-view-encapsulation": "error",
    // Custom rule to warn about inline templates (if available in your linter)
  }
}
```

5. Update Angular CLI schematics to always generate separate files:

```json
// angular.json - Update existing schematics section
{
  "schematics": {
    "@schematics/angular:component": {
      "style": "scss",                // ✅ Already present
      "inlineTemplate": false,        // ⬅️ ADD THIS
      "inlineStyle": false            // ⬅️ ADD THIS
    },
    "@schematics/angular:directive": {
      "standalone": false
    },
    "@schematics/angular:pipe": {
      "standalone": false
    }
  }
}
```

**Why this is a Quick Win**:
- Can be done incrementally (fix components as you touch them)
- Clear, measurable improvement
- Prevents future violations
- Dramatically improves code quality and maintainability

---

### 7. Set Up Testing Infrastructure & Standards
**Impact**: ⭐⭐⭐⭐⭐  
**Risk**: ⭐  
**Effort**: 2-3 hours (initial setup)

```bash
# 1. Install Playwright for E2E tests
npm init playwright@latest

# 2. Update karma.conf.js to enforce coverage
# (Add coverage thresholds - see Testing section)

# 3. Create test fixtures and helpers
mkdir -p src/tests/{e2e,fixtures,helpers}

# 4. Add test scripts to package.json
```

```json
// package.json
{
  "scripts": {
    "test": "ng test",
    "test:ci": "ng test --watch=false --code-coverage --browsers=ChromeHeadless",
    "test:coverage": "ng test --watch=false --code-coverage",
    "e2e": "playwright test",
    "e2e:headed": "playwright test --headed",
    "e2e:ui": "playwright test --ui"
  }
}
```

**Actions**:
1. Install Playwright: `npm init playwright@latest`
2. Configure Playwright for headless mode (see Testing section)
3. Update karma.conf.js with coverage thresholds
4. Create GitHub Actions workflow for tests
5. Document testing standards (done in this document!)
6. Start writing tests for critical paths first

**Why this is a Quick Win**:
- Prevents future code without tests
- Catches bugs before production
- Enables confident refactoring
- **Most important improvement for long-term maintainability**

---

## 🧪 Testing Standards & Implementation Guide

### Overview

Testing is **CRITICAL** for application reliability, maintainability, and developer confidence. This section provides comprehensive testing standards for the Angular application.

---

### Recommended Testing Stack

#### Option A: Keep Current Setup (Easier Migration)
```
✅ Unit Tests:      Jasmine + Karma (already configured)
✅ E2E Tests:       Playwright (add for headless support)
✅ Coverage:        Istanbul (built-in)
```

#### Option B: Modern Stack (Better Long-term)
```
✅ Unit Tests:      Jest (3-5x faster than Karma)
✅ E2E Tests:       Playwright (best headless, cross-browser)
✅ Coverage:        Jest built-in
```

**Recommendation**: Start with **Option A** (less disruption), migrate to Jest later if needed.

---

### Testing Standards by Component Type

#### 1. Services (90% Coverage Required)

**What to Test**:
- All public methods
- Error handling
- HTTP calls (mocked)
- State management
- Business logic

**Example Structure**:
```typescript
// user.service.spec.ts
describe('UserService', () => {
  let service: UserService;
  let httpMock: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [HttpClientTestingModule],
      providers: [UserService]
    });
    service = TestBed.inject(UserService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  describe('getUser()', () => {
    it('should fetch user by id', () => {
      const mockUser = { id: 1, name: 'Test User' };
      
      service.getUser(1).subscribe(user => {
        expect(user).toEqual(mockUser);
      });

      const req = httpMock.expectOne('/api/users/1');
      expect(req.request.method).toBe('GET');
      req.flush(mockUser);
    });

    it('should handle error when user not found', () => {
      service.getUser(999).subscribe({
        error: (error) => {
          expect(error.status).toBe(404);
        }
      });

      const req = httpMock.expectOne('/api/users/999');
      req.flush('Not found', { status: 404, statusText: 'Not Found' });
    });
  });

  describe('updateUser()', () => {
    it('should update user successfully', () => {
      const user = { id: 1, name: 'Updated Name' };
      
      service.updateUser(user).subscribe(response => {
        expect(response).toEqual(user);
      });

      const req = httpMock.expectOne('/api/users/1');
      expect(req.request.method).toBe('PUT');
      req.flush(user);
    });
  });
});
```

---

#### 2. Components (80% Coverage Required)

**What to Test**:
- Component initialization
- Input/Output bindings
- User interactions
- Template rendering
- Conditional rendering
- Form validation

**Example Structure**:
```typescript
// contact-list.component.spec.ts
describe('ContactListComponent', () => {
  let component: ContactListComponent;
  let fixture: ComponentFixture<ContactListComponent>;
  let contactService: jasmine.SpyObj<ContactService>;

  beforeEach(async () => {
    const contactServiceSpy = jasmine.createSpyObj('ContactService', ['getContacts', 'deleteContact']);

    await TestBed.configureTestingModule({
      declarations: [ContactListComponent],
      imports: [HttpClientTestingModule, RouterTestingModule],
      providers: [
        { provide: ContactService, useValue: contactServiceSpy }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(ContactListComponent);
    component = fixture.componentInstance;
    contactService = TestBed.inject(ContactService) as jasmine.SpyObj<ContactService>;
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  describe('ngOnInit', () => {
    it('should load contacts on init', () => {
      const mockContacts = [
        { id: 1, name: 'John Doe' },
        { id: 2, name: 'Jane Smith' }
      ];
      contactService.getContacts.and.returnValue(of(mockContacts));

      component.ngOnInit();

      expect(contactService.getContacts).toHaveBeenCalled();
      expect(component.contacts).toEqual(mockContacts);
      expect(component.loading).toBeFalse();
    });

    it('should handle error when loading contacts fails', () => {
      contactService.getContacts.and.returnValue(throwError(() => new Error('API Error')));

      component.ngOnInit();

      expect(component.error).toBeTruthy();
      expect(component.loading).toBeFalse();
    });
  });

  describe('deleteContact', () => {
    it('should delete contact and refresh list', () => {
      const contactId = 1;
      contactService.deleteContact.and.returnValue(of(void 0));
      contactService.getContacts.and.returnValue(of([]));

      component.deleteContact(contactId);

      expect(contactService.deleteContact).toHaveBeenCalledWith(contactId);
      expect(contactService.getContacts).toHaveBeenCalled();
    });
  });

  describe('template rendering', () => {
    it('should display loading spinner when loading', () => {
      component.loading = true;
      fixture.detectChanges();

      const spinner = fixture.nativeElement.querySelector('.loading-spinner');
      expect(spinner).toBeTruthy();
    });

    it('should display contacts when loaded', () => {
      component.contacts = [
        { id: 1, name: 'John Doe' },
        { id: 2, name: 'Jane Smith' }
      ];
      component.loading = false;
      fixture.detectChanges();

      const contactElements = fixture.nativeElement.querySelectorAll('.contact-item');
      expect(contactElements.length).toBe(2);
    });

    it('should display error message when error occurs', () => {
      component.error = 'Failed to load contacts';
      component.loading = false;
      fixture.detectChanges();

      const errorElement = fixture.nativeElement.querySelector('.error-message');
      expect(errorElement.textContent).toContain('Failed to load contacts');
    });
  });
});
```

---

#### 3. Pipes (95% Coverage Required)

**What to Test**:
- Transform functionality
- Edge cases
- Null/undefined handling

**Example**:
```typescript
// markdown.pipe.spec.ts
describe('MarkdownPipe', () => {
  let pipe: MarkdownPipe;

  beforeEach(() => {
    pipe = new MarkdownPipe();
  });

  it('should create an instance', () => {
    expect(pipe).toBeTruthy();
  });

  it('should transform markdown to HTML', () => {
    const markdown = '# Hello World';
    const result = pipe.transform(markdown);
    expect(result).toContain('<h1>Hello World</h1>');
  });

  it('should handle empty string', () => {
    const result = pipe.transform('');
    expect(result).toBe('');
  });

  it('should handle null', () => {
    const result = pipe.transform(null);
    expect(result).toBe('');
  });

  it('should handle undefined', () => {
    const result = pipe.transform(undefined);
    expect(result).toBe('');
  });

  it('should transform links correctly', () => {
    const markdown = '[Google](https://google.com)';
    const result = pipe.transform(markdown);
    expect(result).toContain('<a href="https://google.com">Google</a>');
  });
});
```

---

#### 4. Guards (100% Coverage Required)

**What to Test**:
- Authorization logic
- Redirect behavior
- Route protection

**Example**:
```typescript
// auth.guard.spec.ts
describe('AuthGuard', () => {
  let guard: AuthGuard;
  let authService: jasmine.SpyObj<AuthService>;
  let router: jasmine.SpyObj<Router>;

  beforeEach(() => {
    const authServiceSpy = jasmine.createSpyObj('AuthService', ['isAuthenticated']);
    const routerSpy = jasmine.createSpyObj('Router', ['navigate']);

    TestBed.configureTestingModule({
      providers: [
        AuthGuard,
        { provide: AuthService, useValue: authServiceSpy },
        { provide: Router, useValue: routerSpy }
      ]
    });

    guard = TestBed.inject(AuthGuard);
    authService = TestBed.inject(AuthService) as jasmine.SpyObj<AuthService>;
    router = TestBed.inject(Router) as jasmine.SpyObj<Router>;
  });

  it('should allow access when authenticated', () => {
    authService.isAuthenticated.and.returnValue(true);

    const result = guard.canActivate();

    expect(result).toBeTrue();
    expect(router.navigate).not.toHaveBeenCalled();
  });

  it('should deny access and redirect when not authenticated', () => {
    authService.isAuthenticated.and.returnValue(false);

    const result = guard.canActivate();

    expect(result).toBeFalse();
    expect(router.navigate).toHaveBeenCalledWith(['/login']);
  });
});
```

---

#### 5. Interceptors (100% Coverage Required)

**What to Test**:
- Request modification
- Response handling
- Error handling

**Example**:
```typescript
// auth.interceptor.spec.ts
describe('AuthInterceptor', () => {
  let interceptor: AuthInterceptor;
  let authService: jasmine.SpyObj<AuthService>;
  let httpMock: HttpTestingController;

  beforeEach(() => {
    const authServiceSpy = jasmine.createSpyObj('AuthService', ['getToken']);

    TestBed.configureTestingModule({
      imports: [HttpClientTestingModule],
      providers: [
        AuthInterceptor,
        { provide: AuthService, useValue: authServiceSpy }
      ]
    });

    interceptor = TestBed.inject(AuthInterceptor);
    authService = TestBed.inject(AuthService) as jasmine.SpyObj<AuthService>;
    httpMock = TestBed.inject(HttpTestingController);
  });

  it('should add auth token to request headers', () => {
    authService.getToken.and.returnValue('test-token');

    // Make an HTTP request
    const http = TestBed.inject(HttpClient);
    http.get('/api/test').subscribe();

    const req = httpMock.expectOne('/api/test');
    expect(req.request.headers.has('Authorization')).toBeTrue();
    expect(req.request.headers.get('Authorization')).toBe('Bearer test-token');
    
    req.flush({});
  });

  it('should not add header when no token', () => {
    authService.getToken.and.returnValue(null);

    const http = TestBed.inject(HttpClient);
    http.get('/api/test').subscribe();

    const req = httpMock.expectOne('/api/test');
    expect(req.request.headers.has('Authorization')).toBeFalse();
    
    req.flush({});
  });
});
```

---

### E2E Testing with Playwright

#### Setup Playwright

```bash
# Install Playwright
npm init playwright@latest

# Install browsers (including headless)
npx playwright install
```

#### Example E2E Tests

```typescript
// tests/e2e/contact-management.spec.ts
import { test, expect } from '@playwright/test';

test.describe('Contact Management', () => {
  test.beforeEach(async ({ page }) => {
    // Login before each test
    await page.goto('/login');
    await page.fill('[data-testid="email"]', 'test@example.com');
    await page.fill('[data-testid="password"]', 'password123');
    await page.click('[data-testid="login-button"]');
    await page.waitForURL('/');
  });

  test('should display contact list', async ({ page }) => {
    await page.goto('/partnerships/contacts');
    
    await expect(page.locator('h1')).toContainText('Contacts');
    await expect(page.locator('.contact-item')).toHaveCount.greaterThan(0);
  });

  test('should create new contact', async ({ page }) => {
    await page.goto('/partnerships/contacts');
    
    await page.click('[data-testid="new-contact-button"]');
    await page.fill('[data-testid="contact-name"]', 'John Doe');
    await page.fill('[data-testid="contact-email"]', 'john@example.com');
    await page.fill('[data-testid="contact-phone"]', '+1234567890');
    await page.click('[data-testid="save-contact"]');
    
    await expect(page.locator('.success-message')).toBeVisible();
    await expect(page.locator('.contact-item')).toContainText('John Doe');
  });

  test('should edit existing contact', async ({ page }) => {
    await page.goto('/partnerships/contacts');
    
    await page.click('.contact-item:first-child [data-testid="edit-button"]');
    await page.fill('[data-testid="contact-name"]', 'Jane Doe Updated');
    await page.click('[data-testid="save-contact"]');
    
    await expect(page.locator('.success-message')).toBeVisible();
    await expect(page.locator('.contact-item:first-child')).toContainText('Jane Doe Updated');
  });

  test('should delete contact', async ({ page }) => {
    await page.goto('/partnerships/contacts');
    
    const initialCount = await page.locator('.contact-item').count();
    
    await page.click('.contact-item:first-child [data-testid="delete-button"]');
    await page.click('[data-testid="confirm-delete"]');
    
    await expect(page.locator('.success-message')).toBeVisible();
    await expect(page.locator('.contact-item')).toHaveCount(initialCount - 1);
  });

  test('should search contacts', async ({ page }) => {
    await page.goto('/partnerships/contacts');
    
    await page.fill('[data-testid="search-input"]', 'John');
    await page.waitForTimeout(500); // Debounce
    
    const contacts = page.locator('.contact-item');
    await expect(contacts.first()).toContainText('John');
  });
});
```

#### Playwright Configuration for Headless

```typescript
// playwright.config.ts
import { defineConfig, devices } from '@playwright/test';

export default defineConfig({
  testDir: './tests/e2e',
  fullyParallel: true,
  forbidOnly: !!process.env.CI,
  retries: process.env.CI ? 2 : 0,
  workers: process.env.CI ? 1 : undefined,
  reporter: 'html',
  
  use: {
    baseURL: 'http://localhost:44426',
    trace: 'on-first-retry',
    screenshot: 'only-on-failure',
  },

  projects: [
    {
      name: 'chromium',
      use: { 
        ...devices['Desktop Chrome'],
        headless: true  // ⬅️ Headless mode
      },
    },
    {
      name: 'firefox',
      use: { 
        ...devices['Desktop Firefox'],
        headless: true
      },
    },
    {
      name: 'webkit',
      use: { 
        ...devices['Desktop Safari'],
        headless: true
      },
    },
  ],

  webServer: {
    command: 'npm run start',
    url: 'http://localhost:44426',
    reuseExistingServer: !process.env.CI,
  },
});
```

---

### Coverage Requirements

#### Minimum Coverage Targets

| Component Type | Coverage Target | Critical |
|---------------|----------------|----------|
| Services | 90% | Yes |
| Components | 80% | Yes |
| Pipes | 95% | Yes |
| Directives | 90% | Yes |
| Guards | 100% | Yes |
| Interceptors | 100% | Yes |
| Models | N/A | - |
| **Overall** | **80%** | **Yes** |

#### Enforcing Coverage

**In Karma**:
```javascript
// karma.conf.js
coverageReporter: {
  dir: require('path').join(__dirname, './coverage'),
  subdir: '.',
  reporters: [
    { type: 'html' },
    { type: 'text-summary' },
    { type: 'lcovonly' }
  ],
  check: {
    global: {
      statements: 80,
      branches: 75,
      functions: 80,
      lines: 80
    },
    each: {
      statements: 70,
      branches: 65,
      functions: 70,
      lines: 70
    }
  }
}
```

**In Jest** (if migrating):
```json
// package.json
{
  "jest": {
    "coverageThreshold": {
      "global": {
        "branches": 75,
        "functions": 80,
        "lines": 80,
        "statements": 80
      }
    }
  }
}
```

---

### CI/CD Integration

#### GitHub Actions Example

```yaml
# .github/workflows/test.yml
name: Tests

on: [push, pull_request]

jobs:
  unit-tests:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v3
      - uses: actions/setup-node@v3
        with:
          node-version: '18'
          cache: 'npm'
      
      - name: Install dependencies
        run: npm ci
      
      - name: Run unit tests
        run: npm test -- --watch=false --code-coverage --browsers=ChromeHeadless
      
      - name: Upload coverage
        uses: codecov/codecov-action@v3
        with:
          files: ./coverage/lcov.info

  e2e-tests:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v3
      - uses: actions/setup-node@v3
        with:
          node-version: '18'
          cache: 'npm'
      
      - name: Install dependencies
        run: npm ci
      
      - name: Install Playwright browsers
        run: npx playwright install --with-deps
      
      - name: Run E2E tests
        run: npx playwright test
      
      - name: Upload test results
        if: always()
        uses: actions/upload-artifact@v3
        with:
          name: playwright-report
          path: playwright-report/
```

---

### Test Data Management

#### Test Fixtures

```typescript
// tests/fixtures/contact.fixtures.ts
export const mockContacts = [
  {
    id: 1,
    name: 'John Doe',
    email: 'john@example.com',
    phone: '+1234567890',
    organization: 'ACME Corp'
  },
  {
    id: 2,
    name: 'Jane Smith',
    email: 'jane@example.com',
    phone: '+0987654321',
    organization: 'Tech Inc'
  }
];

export const createMockContact = (overrides = {}) => ({
  id: 1,
  name: 'Test Contact',
  email: 'test@example.com',
  phone: '+1111111111',
  ...overrides
});
```

#### Test Helpers

```typescript
// tests/helpers/test-utils.ts
export class TestUtils {
  static createComponentWithInputs<T>(
    component: Type<T>,
    inputs: Partial<T>
  ): ComponentFixture<T> {
    const fixture = TestBed.createComponent(component);
    Object.assign(fixture.componentInstance, inputs);
    fixture.detectChanges();
    return fixture;
  }

  static clickButton(fixture: ComponentFixture<any>, selector: string): void {
    const button = fixture.nativeElement.querySelector(selector);
    button.click();
    fixture.detectChanges();
  }

  static setInputValue(
    fixture: ComponentFixture<any>,
    selector: string,
    value: string
  ): void {
    const input = fixture.nativeElement.querySelector(selector);
    input.value = value;
    input.dispatchEvent(new Event('input'));
    fixture.detectChanges();
  }
}
```

---

### Testing Best Practices

#### DO's ✅

1. **Test behavior, not implementation**
   ```typescript
   // ✅ Good
   it('should show error message when login fails', () => {
     // Test what user sees
   });
   
   // ❌ Bad
   it('should call handleError method', () => {
     // Testing internal implementation
   });
   ```

2. **Use descriptive test names**
   ```typescript
   // ✅ Good
   it('should disable submit button when form is invalid', () => {});
   
   // ❌ Bad
   it('test button', () => {});
   ```

3. **Follow AAA pattern** (Arrange, Act, Assert)
   ```typescript
   it('should add two numbers', () => {
     // Arrange
     const calculator = new Calculator();
     
     // Act
     const result = calculator.add(2, 3);
     
     // Assert
     expect(result).toBe(5);
   });
   ```

4. **Test edge cases**
   ```typescript
   it('should handle null input', () => {});
   it('should handle empty array', () => {});
   it('should handle maximum value', () => {});
   ```

5. **Mock external dependencies**
   ```typescript
   const httpMock = jasmine.createSpyObj('HttpClient', ['get', 'post']);
   ```

#### DON'Ts ❌

1. **Don't test Angular framework**
   ```typescript
   // ❌ Bad - testing Angular's router
   it('should navigate to route', () => {
     expect(router.navigate).toHaveBeenCalled();
   });
   ```

2. **Don't write tests that depend on each other**
   ```typescript
   // ❌ Bad
   it('test 1', () => { sharedState.value = 5; });
   it('test 2', () => { expect(sharedState.value).toBe(5); }); // Depends on test 1
   ```

3. **Don't have multiple assertions for different behaviors**
   ```typescript
   // ❌ Bad
   it('should work', () => {
     expect(component.isValid()).toBeTrue();
     expect(component.isSubmitted()).toBeFalse();
     expect(component.errorMessage()).toBe('');
   });
   
   // ✅ Good - split into separate tests
   ```

4. **Don't use real HTTP calls**
   ```typescript
   // ❌ Bad
   it('should fetch users', async () => {
     const users = await http.get('https://api.example.com/users');
   });
   
   // ✅ Good - use HttpTestingController
   ```

---

### Test Organization

```
src/
├── app/
│   ├── core/
│   │   ├── services/
│   │   │   ├── auth/
│   │   │   │   ├── auth.service.ts
│   │   │   │   └── auth.service.spec.ts      ⬅️ Unit test
│   │   │   └── ...
│   │   ├── guards/
│   │   │   ├── auth.guard.ts
│   │   │   └── auth.guard.spec.ts            ⬅️ Unit test
│   │   └── interceptors/
│   │       ├── auth.interceptor.ts
│   │       └── auth.interceptor.spec.ts      ⬅️ Unit test
│   ├── shared/
│   │   ├── components/
│   │   │   └── phone-input/
│   │   │       ├── phone-input.component.ts
│   │   │       ├── phone-input.component.html
│   │   │       ├── phone-input.component.scss
│   │   │       └── phone-input.component.spec.ts  ⬅️ Unit test
│   │   └── pipes/
│   │       ├── markdown.pipe.ts
│   │       └── markdown.pipe.spec.ts         ⬅️ Unit test
│   └── features/
│       └── partnerships/
│           └── contacts/
│               ├── components/
│               │   └── contact-list/
│               │       ├── contact-list.component.ts
│               │       ├── contact-list.component.html
│               │       ├── contact-list.component.scss
│               │       └── contact-list.component.spec.ts  ⬅️ Unit test
│               └── services/
│                   ├── contact.service.ts
│                   └── contact.service.spec.ts    ⬅️ Unit test
├── tests/
│   ├── e2e/                                       ⬅️ E2E tests
│   │   ├── auth.spec.ts
│   │   ├── contact-management.spec.ts
│   │   └── partner-management.spec.ts
│   ├── fixtures/                                  ⬅️ Test data
│   │   ├── contact.fixtures.ts
│   │   └── partner.fixtures.ts
│   └── helpers/                                   ⬅️ Test utilities
│       └── test-utils.ts
└── playwright.config.ts                           ⬅️ E2E configuration
```

---

## 📚 References & Best Practices

### Angular Official Style Guide
- [Angular Coding Style Guide](https://angular.io/guide/styleguide)
- [Angular Architecture Best Practices](https://angular.io/guide/architecture)
- [Angular Testing Guide](https://angular.io/guide/testing)

### Testing Resources
- **Jasmine**: [https://jasmine.github.io/](https://jasmine.github.io/) - Official Jasmine documentation
- **Karma**: [https://karma-runner.github.io/](https://karma-runner.github.io/) - Karma test runner
- **Playwright**: [https://playwright.dev/](https://playwright.dev/) - Modern E2E testing with headless support
- **Jest** (alternative): [https://jestjs.io/](https://jestjs.io/) - Fast JavaScript testing framework
- **Testing Library**: [https://testing-library.com/angular](https://testing-library.com/angular) - Better testing practices
- **Test Coverage**: [https://istanbul.js.org/](https://istanbul.js.org/) - Istanbul code coverage tool

### Key Angular Concepts
- **Smart vs Dumb Components**: Container components (smart) vs presentational (dumb)
- **Feature Modules**: Organize by business domains
- **Lazy Loading**: Load features on demand
- **Barrel Exports**: Use index.ts for clean imports
- **Test-Driven Development (TDD)**: Write tests before code
- **Behavior-Driven Development (BDD)**: Write tests as specifications

### Recommended Reading
- "Angular Architecture Best Practices" by Angular Team
- "Scalable Angular Applications" patterns
- NgRx/Store documentation (if considering state management)
- "Testing Angular Applications" by Jesse Palmer
- "Effective Unit Testing" by Lasse Koskela
- "Growing Object-Oriented Software, Guided by Tests" by Steve Freeman & Nat Pryce

---

## 🤝 Team Guidelines

### For New Features
1. Create feature folder: `features/feature-name/`
2. Follow structure: `components/`, `models/`, `services/`, `resolvers/`
3. Create `feature-name.routes.ts` for routing
4. Add to main routes with lazy loading if large

### For Components (Mandatory Pattern)
1. **Always use separate files** - no inline templates or styles
2. Create component folder: `component-name/`
3. Required files:
   - `component-name.component.ts` - Component logic
   - `component-name.component.html` - Template (always separate)
   - `component-name.component.scss` - Styles (always separate, use SCSS)
   - `component-name.component.spec.ts` - Unit tests
4. Use Angular CLI to generate: `ng generate component component-name`
5. Exception: Only for 1-2 line templates with no styles (must be justified)

### For Shared Components
1. Only make shared if used by 3+ features
2. Place in appropriate category folder
3. Document with README.md
4. Create comprehensive unit tests
5. Add to shared.module exports

### For Services
1. Singleton app-wide → `core/services/{category}/`
2. Reusable utility → `shared/services/{category}/`
3. Feature-specific → `features/{feature}/services/`
4. Use Providedln: 'root' for singletons

### For Testing (Mandatory)
1. **Every new code must have tests** - no exceptions
2. Write tests BEFORE or WITH code (TDD or concurrent)
3. Minimum coverage requirements:
   - Services: 90%
   - Components: 80%
   - Pipes/Directives: 95%
   - Guards/Interceptors: 100%
4. Test file must be in same folder as source file
5. Use descriptive test names: `should [expected behavior] when [condition]`
6. Follow AAA pattern: Arrange, Act, Assert
7. Mock all external dependencies (HTTP, services, etc.)
8. Test both success and error paths
9. Run tests before committing: `npm test`
10. Ensure no test failures before PR

### Code Review Checklist
- [ ] Files in correct folder per architecture
- [ ] Follows feature structure pattern
- [ ] Imports use path aliases (@shared, @core, etc.)
- [ ] Services properly scoped (root vs feature)
- [ ] Components properly categorized (smart vs dumb)
- [ ] **Components use separate files** (no inline templates/styles)
- [ ] Component files follow naming convention (*.component.ts/html/scss)
- [ ] **All new code has accompanying tests** (.spec.ts files)
- [ ] **Tests actually test the code** (not just boilerplate)
- [ ] **Coverage meets minimum thresholds** (check coverage report)
- [ ] **All tests pass** (no skipped/pending tests without justification)
- [ ] No circular dependencies
- [ ] Barrel exports updated if needed

---

## 📈 Migration Timeline Estimate

| Phase | Tasks | Estimated Time | Risk Level |
|-------|-------|---------------|------------|
| **Phase 0A**: Testing Setup | Install Playwright, configure coverage, CI/CD | 2-3 hours | 🟢 Low |
| **Phase 0B**: Component Cleanup | Extract inline templates/styles to files | 4-8 hours | 🟢 Low |
| **Phase 1**: Foundation | Move assets, themes; organize core | 2-4 hours | 🟢 Low |
| **Phase 2**: Shared Module | Reorganize components, services | 1-2 days | 🟡 Medium |
| **Phase 3**: Features | Create new features, reorganize existing | 2-3 days | 🟠 High |
| **Phase 4**: Layouts | Organize topbar, other layout components | 4-6 hours | 🟡 Medium |
| **Phase 5**: Updates | Fix imports, routes, update tests | 1-2 days | 🟠 High |
| **Phase 6**: Test Creation | Write tests for existing code (incremental) | 2-4 weeks | 🟡 Medium |
| **Testing & QA** | Full regression testing | 1-2 days | - |

**Total Estimated Time**: 
- Architecture Migration: 1.5-2 weeks
- Test Creation: 2-4 weeks (can overlap with architecture work)
- **Combined**: 3-6 weeks for complete overhaul

**Recommendation**: 
- **START WITH PHASE 0A** (testing setup) - most critical!
- Then Phase 0B (component cleanup) - can be done incrementally
- Update `angular.json` immediately to prevent future inline templates
- **Write tests as you refactor** - don't defer testing to the end
- Do remaining phases incrementally, feature by feature, testing after each change
- Aim for 80% coverage within 4 weeks

**Parallel Work Strategy**:
1. **Week 1**: Testing setup + Component cleanup + Start critical tests
2. **Week 2**: Foundation + Shared reorganization + Write shared component tests
3. **Week 3**: Feature reorganization + Write feature tests
4. **Week 4**: Layouts + Updates + Write layout tests
5. **Weeks 5-6**: Complete test coverage for remaining code

---

## 🎯 Success Metrics

After reorganization, you should see:

- ✅ **Faster onboarding**: New developers understand structure in < 1 hour
- ✅ **Reduced search time**: Finding files takes < 30 seconds
- ✅ **Fewer merge conflicts**: Clear ownership boundaries
- ✅ **Better code reviews**: Reviewers know where code should go
- ✅ **Consistent component pattern**: 100% of components use separate .html/.scss files
- ✅ **Better IDE support**: Full syntax highlighting and autocomplete in templates
- ✅ **Comprehensive test coverage**: 80%+ overall coverage
- ✅ **Confident refactoring**: Tests catch regressions immediately
- ✅ **Fewer production bugs**: Issues caught in CI/CD before deployment
- ✅ **Faster builds**: Better tree-shaking with proper modules
- ✅ **Improved maintainability**: Changes localized to specific features
- ✅ **Scalability**: Easy to add new features without confusion

### Measurable KPIs

#### Architecture Metrics

**Before Migration**:
```bash
# Run these commands to establish baseline
find src/app -name "*.component.ts" | wc -l                    # Total components
grep -r "template:" src/app --include="*.ts" | wc -l           # Inline templates
grep -r "styles:" src/app --include="*.ts" | wc -l             # Inline styles
```

**After Migration Goals**:
- Inline templates: 0 (or < 5 justified exceptions)
- Inline styles: 0 (or < 5 justified exceptions)
- Component file structure compliance: 100%
- All services categorized: 100%
- No "features/shared" folder: ✅
- All tours in assets: ✅

#### Testing Metrics

**Before Migration** (Current State):
```bash
# Audit current test coverage
npm run test:coverage

# Count missing test files
COMPONENTS=$(find src/app -name "*.component.ts" ! -name "*.spec.ts" | wc -l)
COMPONENT_TESTS=$(find src/app -name "*.component.spec.ts" | wc -l)
SERVICES=$(find src/app -name "*.service.ts" ! -name "*.spec.ts" | wc -l)
SERVICE_TESTS=$(find src/app -name "*.service.spec.ts" | wc -l)

echo "Components: $COMPONENTS, Tests: $COMPONENT_TESTS"
echo "Services: $SERVICES, Tests: $SERVICE_TESTS"
```

**Expected Current State** (need to verify):
- Overall coverage: < 20% (estimated)
- Components with tests: < 30%
- Services with tests: < 40%
- Guards with tests: < 50%
- Interceptors with tests: < 50%
- Pipes with tests: < 30%
- E2E tests: 0

**After Migration Goals**:
- ✅ Overall coverage: ≥ 80%
- ✅ Components with tests: 100%
- ✅ Services with tests: 100%
- ✅ Guards with tests: 100%
- ✅ Interceptors with tests: 100%
- ✅ Pipes with tests: 100%
- ✅ E2E tests: 10+ critical flows covered
- ✅ Tests run in CI/CD: Yes
- ✅ Coverage enforced: Yes (build fails < 80%)
- ✅ All tests passing: Yes (no skipped/pending)

#### Quality Metrics

| Metric | Before | Target | 
|--------|--------|--------|
| Production bugs/month | ? | -50% |
| Time to onboard developer | ? | < 1 day |
| Time to find code | ? | < 30 sec |
| Time to implement feature | ? | -20% |
| Code review time | ? | -30% |
| Refactoring confidence | Low | High |
| Build time | ? | No change |
| Test execution time | N/A | < 5 min |

#### Track Progress

**Weekly Progress Dashboard**:
```bash
#!/bin/bash
# save as: scripts/check-progress.sh

echo "=== Architecture Progress ==="
echo "Inline templates: $(grep -r "template:" src/app --include="*.ts" | wc -l)"
echo "Inline styles: $(grep -r "styles:" src/app --include="*.ts" | wc -l)"
echo ""

echo "=== Testing Progress ==="
echo "Total components: $(find src/app -name "*.component.ts" ! -name "*.spec.ts" | wc -l)"
echo "Components with tests: $(find src/app -name "*.component.spec.ts" | wc -l)"
echo "Total services: $(find src/app -name "*.service.ts" ! -name "*.spec.ts" | wc -l)"
echo "Services with tests: $(find src/app -name "*.service.spec.ts" | wc -l)"
echo ""

echo "=== Running Coverage Report ==="
npm run test:coverage --silent
```

Run this weekly to track progress toward goals!

---

## 📞 Support & Questions

For questions about this architecture:
1. Refer to Angular Style Guide first
2. Check this document's decision matrix
3. Discuss with team lead
4. Document decisions in ADR (Architecture Decision Records)

---

**Document Version**: 1.0  
**Last Updated**: October 11, 2025  
**Author**: AI Code Analysis  
**Status**: Proposed - Pending Team Review

