# Page Context Service

## What It Does

The `PageContextService` automatically extracts data from Angular components and sends it to the AI Assistant, giving the AI full context about what the user is viewing on the page.

## Why It Exists

When users ask the AI Assistant questions, the AI needs to know:
- What page they're on
- What data is currently loaded (partner details, contact list, interactions, etc.)
- What entities they're viewing

Without this context, the AI can only see the URL, which isn't enough information to provide helpful, data-aware responses.

## How It Works

1. Components register themselves by calling `this.pageContextService.setComponentData(this)` in `ngOnInit`
2. When the AI Assistant opens, it automatically calls `getPageContextForAI()` 
3. The service extracts all public data properties from the registered component
4. **It automatically detects and extracts Angular signal values** (like `partner()`, `contacts()`, etc.)
5. It filters out Angular internals, services, and methods
6. It limits array/object depth to prevent huge payloads
7. The clean data is sent to the AI along with the user's question

## Component Implementation

Add these two lines to any component that should provide context to the AI:

### In imports:
```typescript
import { PageContextService } from '../../../../../common/services/page-context.service';
```

### In the component:
```typescript
private pageContextService = inject(PageContextService);
```

### In ngOnInit:
```typescript
ngOnInit() {
  this.pageContextService.setComponentData(this);
  // ... rest of init logic
}
```

### In ngOnDestroy:
```typescript
ngOnDestroy() {
  this.pageContextService.clearComponentData();
  // ... rest of cleanup logic
}
```

## That's It!

No need to specify which fields to send. No need to update the service when adding new properties. The service automatically extracts everything relevant.

## Already Implemented In

- Partner View (`/partnerships/partners/:id`)
- Contact View (`/partnerships/contacts/:id`)
- Interaction Detail (`/interactions/:id`)
- Partner List (`/partnerships/partners`)
- Contact List (`/partnerships/contacts`)
- Interaction List (`/partnerships/interactions`)

## Configuration Options

When calling `getPageContextForAI()`, you can optionally configure:
- `maxArrayLength`: Limit array items (default: 20)
- `maxDepth`: Limit object nesting depth (default: 3)
- `includePrivateProps`: Include properties starting with `_` (default: false)

These are already configured in `ai-assistant-panel.component.ts` and rarely need changing.
