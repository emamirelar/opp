---
name: Opportunity Alt Page
overview: Create a new `OpportunityAltComponent` page accessible at `/partnerships/opportunities-alt` with a left-nav menu entry, serving as a clean-room rebuild shell using only PrimeNG and existing shared components.
todos:
  - id: create-component
    content: Create OpportunityAltComponent (TS, HTML, SCSS) in `features/partnerships/opportunities-alt/`
    status: completed
  - id: create-routes
    content: Create `opportunities-alt.routes.ts` with empty, :recordId, and :recordId/:section routes
    status: completed
  - id: register-route
    content: Add `opportunities-alt` route to `partnerships.routes.ts`
    status: completed
  - id: add-menu-item
    content: Add 'Opportunities Alt' menu entry in `sidebar.component.ts`
    status: completed
  - id: add-translations
    content: Add `title.opportunitiesAlt` key to all 4 i18n JSON files
    status: completed
isProject: false
---

# Opportunity Alt Page — Clean-Room Rebuild Shell

## Goal

Create a new route `/partnerships/opportunities-alt` with a left-nav menu entry under Partnerships, hosting a minimal `OpportunityAltComponent` that loads opportunity data and provides a clean starting point for the rebuild using only PrimeNG components and existing shared components.

## Architecture

```mermaid
flowchart TD
    subgraph routing [Routing Chain]
        AppRoutes["app.routes.ts"] -->|"/partnerships/*"| PartnershipsRoutes["partnerships.routes.ts"]
        PartnershipsRoutes -->|"/opportunities-alt"| AltRoutes["opportunities-alt.routes.ts"]
        AltRoutes -->|""| AltComponent["OpportunityAltComponent"]
        AltRoutes -->|":recordId"| AltComponent
    end
    subgraph nav [Left Navigation]
        Sidebar["sidebar.component.ts"] -->|"new menu item"| MenuEntry["Opportunities Alt"]
    end
    subgraph component [Component]
        AltComponent -->|"injects"| OppService["OpportunityService"]
        AltComponent -->|"uses"| PrimeNG["PrimeNG Components"]
        AltComponent -->|"uses"| SharedComponents["Shared App Components"]
    end
```



## Files to Create

### 1. Component folder and files

Location: `UNOPS.PAO.ClientApp/src/app/features/partnerships/opportunities-alt/`

- `**opportunity-alt.component.ts**` — Standalone component with `OnPush`, injects `OpportunityService`, `ActivatedRoute`, `Router`. Loads opportunity data by `recordId` from route params. Exposes `opportunity` signal and `loading` signal. Imports common PrimeNG modules (PanelModule, ButtonModule, TagModule, MessageModule, etc.) and shared components.
- `**opportunity-alt.component.html**` — Minimal starter template with:
  - A loading spinner (use `p-progressSpinner`)
  - A header area displaying the opportunity title and stage via `p-tag`
  - A placeholder content area with a `p-message` saying "Opportunity Alt — ready to build"
  - Uses only PrimeNG components and Tailwind utility classes for layout
- `**opportunity-alt.component.scss**` — Empty or near-empty (Tailwind-first)

### 2. Route file

- `**opportunities-alt.routes.ts**` — Defines routes matching the pattern from the existing opportunities routes:

```typescript
export const OPPORTUNITIES_ALT_ROUTES: Routes = [
  {
    path: '',
    component: OpportunityAltComponent,
    canActivate: [authGuard],
  },
  {
    path: ':recordId',
    component: OpportunityAltComponent,
    canActivate: [authGuard],
  },
  {
    path: ':recordId/:section',
    component: OpportunityAltComponent,
    canActivate: [authGuard],
  },
];
```

## Files to Modify

### 3. Register the route

In [partnerships.routes.ts](UNOPS.PAO.ClientApp/src/app/features/partnerships/partnerships.routes.ts), add a new entry **before** the existing `opportunities` route:

```typescript
{
  path: 'opportunities-alt',
  loadChildren: () => import('@partnerships/opportunities-alt/opportunities-alt.routes')
    .then(m => m.OPPORTUNITIES_ALT_ROUTES),
  canActivate: [authGuard],
  data: { breadcrumb: 'Opportunities Alt' }
},
```

### 4. Add menu item to left navigation

In [sidebar.component.ts](UNOPS.PAO.ClientApp/src/app/layouts/components/sidebar/sidebar.component.ts), in the `initializeMenuItems` method, add a new entry in the `title.partnerships` items array after the `opportunityItem`:

```typescript
{
  label: 'title.opportunitiesAlt',
  icon: 'lightbulb',
  routerLink: ['/partnerships/opportunities-alt']
},
```

### 5. Add translation keys

Add `"title.opportunitiesAlt": "Opportunities Alt"` to all 4 translation files:

- [en.json](UNOPS.PAO.ClientApp/src/assets/i18n/en.json)
- [fr.json](UNOPS.PAO.ClientApp/src/assets/i18n/fr.json)
- [span.json](UNOPS.PAO.ClientApp/src/assets/i18n/span.json)
- [pt.json](UNOPS.PAO.ClientApp/src/assets/i18n/pt.json)

For non-English files, use the English text for now (the user can update translations later).

## What the Starter Component Will Include

The `OpportunityAltComponent` will be a **minimal shell** that:

1. Reads `recordId` from route params
2. Loads the opportunity via `OpportunityService`
3. Displays a basic header with the opportunity name and stage
4. Has an empty content area ready for the user to build sections using clean PrimeNG components

This gives the user a working page at `https://localhost:51654/partnerships/opportunities-alt` (list) and `https://localhost:51654/partnerships/opportunities-alt/:recordId` (detail) with the opportunity data loaded and available, ready to be composed with clean library components.