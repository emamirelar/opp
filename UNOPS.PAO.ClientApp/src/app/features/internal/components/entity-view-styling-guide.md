# Entity View Styling Guide

This document outlines the standardized styling patterns used in entity view components, based on the partner-view.component.html implementation. Use this guide to ensure consistency across all entity views (Contact, Partner, Interaction, etc.).

## Overall Layout Structure

### Main Container
```html
<div class="flex flex-col gap-8">
  <div class="flex flex-col xl:flex-row gap-8">
    <!-- Main content area -->
    <div class="flex flex-col gap-8" [ngClass]="showAiPanel ? 'xl:w-1/2' : 'w-full'">
      <!-- Entity information panels -->
    </div>
    <!-- AI panel (conditional) -->
    <div class="md:w-1/2 flex flex-col gap-8" *ngIf="showAiPanel">
      <!-- AI panels -->
    </div>
  </div>
</div>
```

## Panel Structure

### Main Information Panel
```html
<p-panel class="{{infoLoading() ? 'opacity-50' : ''}} unops-card unops-surface-elevated unops-rounded-lg unops-shadow-lg">
  <ng-template pTemplate="header">
    <div class="flex justify-between items-center w-full">
      <div class="flex items-center gap-2">
        <span class="unops-text-headline-medium">{{ panelTitle }}</span>
        <app-entity-tags [tags]="recordData().tags"></app-entity-tags>
      </div>
      <div class="flex items-center gap-2">
        <!-- Action buttons -->
      </div>
    </div>
  </ng-template>
  <!-- Panel content -->
</p-panel>
```

## Information Hierarchy

### Primary Information Sections
For the most important information that should stand out:

```html
<div class="unops-p-lg unops-mb-md unops-rounded-lg" 
     style="background-color: var(--unops-surface-cool); border: 1px solid var(--unops-neutral-200);">
  <div class="unops-text-body-large unops-text-secondary unops-mb-sm">{{ sectionLabel }}</div>
  <div class="flex items-center gap-3">
    <div class="unops-flex unops-items-center unops-gap-sm">
      <i class="pi pi-icon-name unops-text-secondary" style="font-size: 1.1rem;"></i>
      <span class="unops-text-body-large unops-text-secondary" 
            style="font-weight: var(--unops-font-weight-semibold);">
        {{ primaryValue }}
      </span>
    </div>
  </div>
</div>
```

### Secondary Information Blocks
For highlighted but less critical information:

```html
<div class="unops-p-md unops-rounded-md unops-mb-sm" 
     style="background-color: var(--unops-neutral-100); border: 1px solid var(--unops-neutral-200);">
  <div class="unops-text-label-large unops-text-muted unops-mb-sm">{{ fieldLabel }}</div>
  <div class="unops-text-body-large unops-text-secondary" 
       style="font-weight: var(--unops-font-weight-semibold);">
    {{ fieldValue }}
  </div>
</div>
```

### Standard Information Rows
For regular field display:

```html
<div class="unops-flex unops-justify-between unops-items-center unops-p-sm unops-rounded-sm unops-border-light">
  <span class="unops-text-label-large unops-text-muted">{{ fieldLabel }}</span>
  <span class="unops-text-body-medium unops-text-secondary">{{ fieldValue }}</span>
</div>
```

### Multi-line Information Rows
For fields with longer content:

```html
<div class="unops-flex unops-justify-between unops-items-start unops-p-sm unops-rounded-sm unops-border-light">
  <span class="unops-text-label-large unops-text-muted" 
        style="flex-shrink: 0; margin-right: var(--unops-spacing-md);">
    {{ fieldLabel }}
  </span>
  <span class="unops-text-body-medium unops-text-secondary text-right" 
        style="line-height: 1.6;">
    {{ longFieldValue }}
  </span>
</div>
```

## Section Headers

### Main Section Headers
```html
<div class="unops-text-headline-small unops-text-secondary unops-mb-md">
  {{ sectionTitle }}
</div>
```

### Sub-section Headers with Icons
```html
<div class="unops-text-headline-small unops-text-secondary unops-mb-md unops-flex unops-items-center unops-gap-sm">
  <i class="pi pi-icon-name unops-text-info" style="font-size: 1rem;"></i>
  {{ sectionTitle }}
</div>
```

### Field Section Headers
```html
<div class="unops-text-label-large unops-text-secondary unops-mb-sm unops-flex unops-items-center unops-gap-sm">
  <i class="pi pi-icon-name unops-text-warning" style="font-size: 1rem;"></i>
  {{ fieldGroupTitle }}
</div>
```

## Special Information Types

### Warning Information
For expiry dates or cautionary information:

```html
<div class="unops-flex unops-justify-between unops-items-center unops-p-sm unops-rounded-sm" 
     style="border: 1px solid var(--unops-warning); background: var(--unops-accent-yellow-soft);">
  <span class="unops-text-label-large unops-text-warning">{{ warningLabel }}</span>
  <span class="unops-text-body-medium unops-text-warning" 
        style="font-weight: var(--unops-font-weight-medium);">
    {{ warningValue }}
  </span>
</div>
```

### Alert Information
For critical information that needs attention:

```html
<div class="unops-card unops-p-md unops-rounded-md unops-mb-md" 
     style="border: 1px solid var(--unops-warning); background: var(--unops-accent-orange-soft);">
  <div class="unops-flex unops-items-start unops-gap-sm unops-mb-sm">
    <i class="pi pi-exclamation-triangle unops-text-warning" 
       style="font-size: 1.1rem; margin-top: 2px;"></i>
    <div class="unops-text-label-large unops-text-warning">{{ alertTitle }}</div>
  </div>
  <div class="unops-text-body-medium unops-pl-lg" 
       style="color: var(--unops-warning-dark);">
    {{ alertMessage }}
  </div>
</div>
```

### Boolean/Checkbox Information
For yes/no or true/false fields:

```html
<div class="unops-flex unops-justify-between unops-items-center unops-p-sm unops-rounded-sm unops-border-light">
  <span class="unops-text-label-large unops-text-muted">{{ booleanFieldLabel }}</span>
  <div class="unops-flex unops-items-center unops-gap-sm">
    @if(booleanValue) {
      <i class="pi pi-check unops-text-success" style="font-size: 1rem;"></i>
      <span class="unops-text-body-medium unops-text-secondary">Yes</span>
    } @else {
      <i class="pi pi-times unops-text-muted" style="font-size: 1rem;"></i>
      <span class="unops-text-body-medium unops-text-muted">No</span>
    }
  </div>
</div>
```

### Featured Boolean Information
For important boolean fields that should be highlighted:

```html
<div class="unops-p-md unops-rounded-md unops-mb-md" 
     style="background-color: var(--unops-neutral-100); border: 1px solid var(--unops-neutral-200);">
  <div class="unops-text-label-large unops-text-muted unops-mb-sm">{{ featuredBooleanLabel }}</div>
  <div class="unops-flex unops-items-center unops-gap-sm">
    @if(featuredBooleanValue) {
      <i class="pi pi-check unops-text-success" style="font-size: 1.2rem;"></i>
      <span class="unops-text-body-large unops-text-secondary" 
            style="font-weight: var(--unops-font-weight-semibold);">Yes</span>
    } @else {
      <i class="pi pi-times unops-text-muted" style="font-size: 1.2rem;"></i>
      <span class="unops-text-body-large unops-text-muted" 
            style="font-weight: var(--unops-font-weight-semibold);">No</span>
    }
  </div>
</div>
```

## Action Buttons

### Header Action Buttons
```html
<div class="flex items-center gap-2">
  @if (hasPermission) {
    <p-button [label]="'button.action' | translate"
              icon="pi pi-icon-name"
              [rounded]="true"
              severity="primary"
              (onClick)="handleAction()"></p-button>
  }
  @if (hasEditPermission) {
    <p-button icon="pi pi-pencil"
              [rounded]="true"
              [text]="true"
              [size]="'small'"
              class="edit-button entity-edit-button"
              (onClick)="handleEditClick()"></p-button>
  }
</div>
```

### See More/Less Functionality
```html
@if (shouldShowSeeMoreButton()) {
  <div class="flex justify-center -mb-4">
    <p-button
      label="{{ 'button.seeMore' | translate }}"
      icon="pi pi-chevron-down"
      [rounded]="true"
      [size]="'small'"
      [text]="true"
      (onClick)="toggleFullContent()"
    />
  </div>
}

@if (shouldShowSeeLessButton()) {
  <div class="flex justify-center -mb-4">
    <p-button
      label="{{ 'button.seeLess' | translate }}"
      icon="pi pi-chevron-up"
      [rounded]="true"
      [size]="'small'"
      [text]="true"
      (onClick)="toggleFullContent()"
    />
  </div>
}
```

## Dividers and Separators

### Standard Divider
```html
<p-divider></p-divider>
```

### Section Divider with Margin
```html
<p-divider class="col-span-full" />
```

## Links and Documents Sections

### Section Header with Add Button
```html
<div class="flex justify-between items-center unops-mb-sm">
  <div class="unops-text-headline-medium unops-text-secondary unops-flex unops-items-center unops-gap-sm">
    <i class="pi pi-link unops-text-info" style="font-size: 1.1rem;"></i>
    {{ 'title.links' | translate }}
  </div>
  @if (hasUpdatePermission) {
    <p-button
      icon="pi pi-plus"
      rounded
      styleClass="p-button-text"
      [label]="'button.addLink' | translate"
      (onClick)="openAddLinkDialog()"
      severity="primary" />
  }
</div>
```

## AI Panel Structure

### AI Panel Container
```html
<div class="md:w-1/2 flex flex-col gap-8" *ngIf="showAiPanel">
  <app-ai-panel
    [title]="'label.entity.summaryTitle'"
    [entityId]="recordId"
    [promptType]="'entity_summary'"
    [aiService]="geminiService"
    (onRefresh)="onSummaryRefresh()"
    (onDataLoaded)="onSummaryLoaded($event)"
    (onError)="onSummaryError($event)"
    class="summary-section"
  />

  <app-ai-panel
    [title]="'label.entity.newsTitle'"
    [entityId]="recordId"
    [promptType]="'entity_news'"
    [aiService]="geminiService"
    (onRefresh)="onNewsRefresh()"
    (onDataLoaded)="onNewsLoaded($event)"
    (onError)="onNewsError($event)"
    class="news-section"
  />
</div>
```

## Color and Icon Guidelines

### Icon Colors by Context
- **Info/Navigation**: `unops-text-info` (blue)
- **Success/Positive**: `unops-text-success` (green)  
- **Warning/Caution**: `unops-text-warning` (orange/yellow)
- **Secondary/Neutral**: `unops-text-secondary` (gray)
- **Muted/Less Important**: `unops-text-muted` (light gray)

### Icon Sizes
- **Large Headers**: `font-size: 1.2rem;`
- **Section Headers**: `font-size: 1.1rem;`
- **Field Labels**: `font-size: 1rem;`
- **Small Elements**: `font-size: 0.9rem;`

### Background Colors
- **Primary Highlight**: `var(--unops-surface-cool)`
- **Secondary Highlight**: `var(--unops-neutral-100)`
- **Warning Background**: `var(--unops-accent-yellow-soft)`
- **Alert Background**: `var(--unops-accent-orange-soft)`

## Responsive Design

### Container Responsiveness
```html
<div class="flex flex-col xl:flex-row gap-8">
  <div class="flex flex-col gap-8" [ngClass]="showAiPanel ? 'xl:w-1/2' : 'w-full'">
```

### Grid Responsiveness
```html
<div class="grid grid-cols-1 md:grid-cols-2 gap-6">
```

### Column Spans
```html
<div class="col-span-9 sm:col-span-3"> <!-- Mobile full width, tablet+ 1/3 width -->
<div class="col-span-9 sm:col-span-6"> <!-- Mobile full width, tablet+ 2/3 width -->
```

## Implementation Checklist

When applying this styling guide to other entity views:

- [ ] Use consistent panel structure with `unops-card unops-surface-elevated unops-rounded-lg unops-shadow-lg`
- [ ] Implement proper information hierarchy (Primary > Secondary > Standard)
- [ ] Apply appropriate icon colors and sizes based on context
- [ ] Use consistent spacing with UNOPS design system classes
- [ ] Implement responsive layout with proper breakpoints
- [ ] Add See More/Less functionality for complex entities
- [ ] Include proper action button styling in headers
- [ ] Use appropriate background colors for different information types
- [ ] Ensure consistent typography hierarchy
- [ ] Implement proper AI panel integration where applicable

## Notes

- Always use UNOPS design system classes (`unops-*`) over custom CSS
- Maintain consistent gap spacing throughout (`gap-8`, `gap-4`, `gap-2`)
- Use semantic colors that match the information context
- Ensure all interactive elements have proper hover states
- Test responsive behavior across all breakpoints
- Validate that all icon sizes and colors are consistent with the design system
