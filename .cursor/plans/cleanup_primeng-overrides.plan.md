---
name: PrimeNG override cleanup
overview: Clean up the layered PrimeNG SCSS overrides that have made the library's theming system redundant. Consolidate design tokens to a single source of truth, migrate SCSS overrides into the preset, eliminate ::ng-deep usages, and establish guardrails to prevent recurrence. Presentation at https://emamirelar.github.io/Opportunityplus_UX/primeng-issues-report.html
todos:
  - id: audit-tokens
    content: Audit all 4 token sources (preset, CSS vars, SCSS vars, Tailwind config) and document which values differ vs. which are identical
    status: pending
  - id: consolidate-tokens
    content: "Phase 1: Make unops-design-tokens.css the single source of truth; update preset, SCSS vars, and Tailwind config to consume from CSS vars instead of hardcoded hex values"
    status: pending
  - id: migrate-buttons
    content: "Phase 2a: Remove .p-button overrides from primeng-unops-theme.scss, verify preset handles all button variants/states correctly, fix preset if needed"
    status: pending
  - id: migrate-inputs
    content: "Phase 2b: Remove .p-inputtext/.p-dropdown/.p-multiselect overrides from primeng-unops-theme.scss, verify preset formField tokens are correct"
    status: pending
  - id: migrate-cards-panels
    content: "Phase 2c: Remove .p-card and .p-panel overrides from primeng-unops-theme.scss, verify preset card/panel tokens"
    status: pending
  - id: migrate-dialogs
    content: "Phase 2d: Remove .p-dialog overrides from primeng-unops-theme.scss, fix mask contradiction (preset says dark, styles.scss forces white)"
    status: pending
  - id: migrate-tables
    content: "Phase 2e: Remove .p-datatable overrides from primeng-unops-theme.scss, verify preset datatable tokens"
    status: pending
  - id: migrate-menus-toasts
    content: "Phase 2f: Remove .p-menu, .p-toast, .p-tooltip, .p-progressbar overrides from primeng-unops-theme.scss"
    status: pending
  - id: fix-font-contradiction
    content: "Fix font contradiction: preset defines Noto Sans, CSS tokens define Inter -- pick one and update preset to match"
    status: pending
  - id: fix-important-vars
    content: Remove !important overrides from _common.scss (icon button width, spinner colors) and styles.scss (mask, tabs) -- move values into preset
    status: pending
  - id: cleanup-utilities
    content: "Remove redundant utility classes from unops-utilities.scss: .unops-button-primary, .unops-button-secondary, .unops-button-text, .unops-card, .unops-card-elevated and their !important counterparts"
    status: pending
  - id: cleanup-token-css-classes
    content: Remove redundant .unops-button-primary and .unops-button-secondary class definitions from unops-design-tokens.css (lines 289-327)
    status: pending
  - id: eliminate-ngdeep-opportunities
    content: "Phase 3a: Remove ::ng-deep from 18 opportunity component SCSS files, verify global styles handle them"
    status: pending
  - id: eliminate-ngdeep-ai
    content: "Phase 3b: Remove ::ng-deep from 6 AI component SCSS files (ai-assistant-panel has 29 usages, ai-panel has 46)"
    status: pending
  - id: eliminate-ngdeep-partners
    content: "Phase 3c: Remove ::ng-deep from 7 partner component files"
    status: pending
  - id: eliminate-ngdeep-layouts
    content: "Phase 3d: Remove ::ng-deep from 5 layout component files (sidebar, topbar, breadcrumb, global-filters, org-unit-selector)"
    status: pending
  - id: eliminate-ngdeep-shared
    content: "Phase 3e: Remove ::ng-deep from 10 shared component files (workflow, tabs, document, timeline, etc.)"
    status: pending
  - id: eliminate-ngdeep-admin
    content: "Phase 3f: Remove ::ng-deep from 10 admin/other component files"
    status: pending
  - id: add-guardrails
    content: "Phase 4: Add PR checklist item and code review rule -- no .p-* overrides in SCSS, no new ::ng-deep, no !important on PrimeNG vars, token changes in one file only"
    status: pending
isProject: true
---

# PrimeNG Override Cleanup

## Problem Statement

The UNOPS Opportunity+ Angular frontend has accumulated 5+ competing styling layers that override PrimeNG's theming system, resulting in ~7,700 lines of redundant code, 62+ files with `::ng-deep`, 40+ files with `!important`, and design tokens duplicated across 4 files. This makes the PrimeNG component library's theming effectively redundant.

Full visual report: [https://emamirelar.github.io/Opportunityplus_UX/primeng-issues-report.html](https://emamirelar.github.io/Opportunityplus_UX/primeng-issues-report.html)

## The Override Cascade (Current State)

```mermaid
flowchart TB
    subgraph cascade [Override Cascade]
        L1["Layer 1: PrimeNG Material Preset -- built-in"]
        L2["Layer 2: unops.preset.ts -- 5,210 lines"]
        L3["Layer 3: primeng-unops-theme.scss -- 885 lines"]
        L4["Layer 4: unops-utilities.scss -- 580 lines + !important"]
        L5["Layer 5: 62+ component SCSS with ::ng-deep"]
    end
    L1 -->|"overridden by"| L2
    L2 -->|"overridden by"| L3
    L3 -->|"overridden by"| L4
    L4 -->|"overridden by"| L5
```



## Target Architecture

```mermaid
flowchart TB
    subgraph target [Clean Architecture]
        T1["PrimeNG Material Preset -- built-in"]
        T2["unops.preset.ts -- UNOPS tokens via definePreset"]
        T3["unops-design-tokens.css -- single source of truth for values"]
        T4["tailwind.config.js -- consumes CSS vars for utility classes"]
    end
    T3 -->|"values flow to"| T2
    T3 -->|"values flow to"| T4
    T1 -->|"customized by"| T2
```



## Phase 1: Consolidate Design Tokens

**Goal:** Single source of truth for all design token values.

### Files to modify

- [unops-design-tokens.css](UNOPS.PAO.ClientApp/src/styles/unops-design-tokens.css) -- keep as canonical source (already has all tokens as CSS custom properties)
- [unops.preset.ts](UNOPS.PAO.ClientApp/src/styles/themes/unops.preset.ts) -- replace hardcoded hex values with references where possible, or document that this file maps CSS var values to PrimeNG token structure
- [unops-design-tokens.scss](UNOPS.PAO.ClientApp/src/styles/unops-design-tokens.scss) -- remove duplicated color/spacing/font variables, replace with `var()` references or delete entirely if unused after Phase 2
- [tailwind.config.js](UNOPS.PAO.ClientApp/tailwind.config.js) -- no duplication of hex values; consume from CSS vars where Tailwind supports it

### Key contradictions to resolve


| Token             | Preset value              | Override value                     | File                               | Resolution              |
| ----------------- | ------------------------- | ---------------------------------- | ---------------------------------- | ----------------------- |
| Mask background   | `rgba(0,0,0,0.32)`        | `rgba(255,255,255,0.5) !important` | `styles.scss` line 17              | Pick one, set in preset |
| Font family       | `'Noto Sans', sans-serif` | `'Inter', 'Noto Sans', ...`        | `unops-design-tokens.css` line 167 | Pick one, update preset |
| Icon button width | `3rem`                    | `2.4rem !important`                | `_common.scss` line 22             | Pick one, set in preset |
| Spinner colors    | PrimeNG default           | `var(--primary-color) !important`  | `_common.scss` lines 24-27         | Set in preset           |
| Tab panel bg      | PrimeNG default           | `transparent !important`           | `styles.scss` line 18              | Set in preset           |


## Phase 2: Migrate SCSS Overrides into Preset

**Goal:** Delete [primeng-unops-theme.scss](UNOPS.PAO.ClientApp/src/styles/primeng-unops-theme.scss) entirely (885 lines).

Work component by component. For each:

1. Comment out the SCSS override block
2. Check if the preset already produces the correct styling
3. If not, fix the preset tokens
4. Verify visually
5. Delete the commented block

### Components to migrate (in order)


| Component                                         | SCSS lines | Preset section                                         |
| ------------------------------------------------- | ---------- | ------------------------------------------------------ |
| `.p-button` (all variants)                        | 9-187      | `components.button`                                    |
| `.p-inputtext` / `.p-dropdown` / `.p-multiselect` | 193-357    | `components.inputtext`, `components.select`            |
| `.p-card`                                         | 363-406    | `components.card`                                      |
| `.p-panel`                                        | 412-447    | `components.panel`                                     |
| `.p-menu`                                         | 453-502    | `components.menu`                                      |
| `.p-dialog`                                       | 508-594    | `components.dialog`                                    |
| `.p-datatable`                                    | 600-656    | `components.datatable`                                 |
| `.p-toast`                                        | 664-705    | `components.toast`                                     |
| `.p-tooltip`                                      | 711-777    | `components.tooltip`                                   |
| `.p-progressbar` / `.p-progress-spinner`          | 783-811    | `components.progressbar`, `components.progressspinner` |
| `.unops-icon-button` utility                      | 818-885    | Use Tailwind classes or preset                         |


### Also delete from [unops-utilities.scss](UNOPS.PAO.ClientApp/src/styles/unops-utilities.scss)

- `.unops-button-primary` (lines 55-84) -- use `p-button` with `severity="primary"`
- `.unops-button-secondary` (lines 87-115) -- use `p-button` with `severity="secondary"`
- `.unops-button-text` (lines 118-143) -- use `p-button` with `variant="text"`
- `.unops-card` / `.unops-card-elevated` (lines 149-170) -- use `p-card` or Tailwind
- Surface/text utilities with `!important` can stay if used in non-PrimeNG contexts, but remove `!important`

### Also clean from [unops-design-tokens.css](UNOPS.PAO.ClientApp/src/styles/unops-design-tokens.css)

- Remove `.unops-button-primary` class (lines 289-307) -- duplicate of utility
- Remove `.unops-button-secondary` class (lines 309-327) -- duplicate of utility
- Remove `.unops-card` class (lines 281-287) -- duplicate of utility

## Phase 3: Eliminate `::ng-deep`

**Goal:** Remove `::ng-deep` from 62+ component files.

With global theming fixed in Phase 2, most `::ng-deep` overrides become unnecessary. For each file:

1. Remove `::ng-deep` wrapper
2. If the style is a legitimate layout concern (not theming), keep it but use `:host` or global styles
3. Verify visually

### Files by area

**Opportunities (18 files):** opportunity-view, overview, where, when, why, what, who, team, dst, collaboration, statement, analysis, documents, ai-comparison, related-items, decision-info-panel, reject-dialog, approve-dialog

**AI (6 files):** ai-assistant-panel (29 usages), ai-panel (46 usages), ai-prompt, ai-content, content-renderer, collapsible-thought

**Partners (7 files):** partner-tree (6 usages), partner-view (18 inline), partner-tree-view, partner-tree-page, partner-tabs, org-structure-dialog, contact-view

**Layouts (5 files):** sidebar, topbar (5 usages), breadcrumb, global-filters-dialog (7 usages), org-unit-selector

**Shared (10 files):** workflow, stage-workflow, responsive-tabs, document-upload, document, timeline (22 usages), tour-control, ai-comparison, dashboard-card, splitter

**Admin/Other (10 files):** entity-manager, user-management, bulk-entity-artifact, search-result, login, sign-up, home-dashboard, interaction-detail, create-opportunity-from-interactions-dialog, duplicate-indicator/summary

## Phase 4: Establish Guardrails

### Code review checklist addition

Add to PR template:

- No `.p-`* class overrides in SCSS -- all PrimeNG styling goes through the preset
- No new `::ng-deep` without explicit approval
- No `!important` on PrimeNG CSS variables
- Design token changes happen in `unops-design-tokens.css` only

### Key files to monitor in reviews

- `src/styles/themes/unops.preset.ts` -- the only place for PrimeNG theming
- `src/styles/unops-design-tokens.css` -- the only place for token values
- Any `*.component.scss` -- should not contain `.p-`* selectors

## Expected Outcome

- ~5,000+ lines of redundant code removed
- Single source of truth for design tokens
- PrimeNG version upgrades become safe
- New components automatically inherit the design system
- Styling changes cost 1x instead of 4-6x

