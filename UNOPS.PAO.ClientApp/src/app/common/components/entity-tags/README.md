# Entity Tags System

The Entity Tags system provides a generic, reusable way to display conditional status and condition tags across all entities in the application.

## Overview

This system consists of:
- **Frontend**: Generic `EntityTagsComponent` and `EntityTag` interface
- **Backend**: Generic `EntityTagModel` with computed tags in Model classes
- **Implementation**: Presentation layer approach for clean separation of concerns

## Frontend Usage

### 1. Make your entity model implement `TaggedEntity`

```typescript
import { EntityTag, TaggedEntity } from '../../../common/models/entity-tag.model';

export interface MyEntity extends TaggedEntity {
  id: number;
  name: string;
  // ... other properties
  // tags is inherited from TaggedEntity
}
```

### 2. Use the EntityTagsComponent in your templates

```html
<div class="flex items-center gap-2">
  <span class="text-xl">My Entity Information</span>
  <app-entity-tags [tags]="recordData().tags"></app-entity-tags>
</div>
```

### 3. Import the component

```typescript
import { EntityTagsComponent } from '../../../../../common/components/entity-tags/entity-tags.component';

@Component({
  selector: 'app-my-entity-view',
  imports: [
    // ... other imports
    EntityTagsComponent,
  ],
  // ...
})
export class MyEntityViewComponent {
  // ...
}
```

## Backend Usage

**Example: Partner Implementation**

### 1. Add computed property to your Model class

```csharp
public class PartnerModel
{
    // ... other properties
    
    // Computed property - automatically calculates tags when accessed
    public List<EntityTagModel>? Tags => CalculateConditionalTags();
    
    /// <summary>
    /// Calculate conditional tags based on partner's current state for frontend display
    /// </summary>
    public List<EntityTagModel> CalculateConditionalTags()
    {
        var tags = new List<EntityTagModel>();
        
        // Partner Status Tags
        if (!string.IsNullOrEmpty(Status))
        {
            var statusColor = Status switch
            {
                "Draft" => "bg-gray-100 text-gray-800",
                "Active" => "bg-blue-100 text-blue-800", 
                "Closed" => "bg-red-100 text-red-800",
                "Archived" => "bg-yellow-100 text-yellow-800",
                _ => "bg-gray-100 text-gray-800"
            };
            tags.Add(new EntityTagModel { Tag = Status, Color = statusColor });
        }
        
        // Due Diligence Expiry Tags
        if (DueDiligenceExpiryDate.HasValue)
        {
            var now = DateTime.UtcNow;
            var expiryDate = DueDiligenceExpiryDate.Value;
            
            if (expiryDate < now)
            {
                tags.Add(new EntityTagModel { Tag = "DD Expired", Color = "bg-red-100 text-red-800" });
            }
            else if (expiryDate <= now.AddMonths(6))
            {
                tags.Add(new EntityTagModel { Tag = "DD Expiring", Color = "bg-yellow-100 text-yellow-800" });
            }
        }
        
        return tags;
    }
}
```

### 2. No AutoMapper configuration needed!

Since `Tags` is a computed property, it automatically calculates tags whenever accessed. No special mapping configuration required.

## Color Classes

Use Tailwind CSS background and text color classes:

### Status Colors
- **Draft/Inactive**: `bg-gray-100 text-gray-800`
- **Active/Success**: `bg-blue-100 text-blue-800` or `bg-green-100 text-green-800`
- **Warning/Expiring**: `bg-yellow-100 text-yellow-800`
- **Error/Expired/Closed**: `bg-red-100 text-red-800`

### Recommended Colors (universally available in Tailwind)
- Gray, Red, Yellow, Green, Blue are the most reliable color classes
- Avoid: Orange, Amber, Indigo, Purple (may not be in all Tailwind builds)
- Always test color combinations for proper contrast

## Features

- ✅ **Responsive**: Tags automatically wrap and resize
- ✅ **Accessible**: Proper color contrast and semantic markup
- ✅ **Reusable**: Works with any entity by adding computed Tags property
- ✅ **Type Safe**: Full TypeScript support with proper interfaces
- ✅ **Performance Optimized**: Uses OnPush change detection
- ✅ **Real-time**: Tags update automatically when entity data changes
- ✅ **Clean Architecture**: Presentation logic stays in presentation layer
- ✅ **Zero Configuration**: No AutoMapper setup needed - just add computed property

## Examples

### Partner Entity (Current Implementation)
- Status tags: Draft (gray), Active (blue), Closed (red), Archived (yellow)
- Approval tags: Approved (green), Pending Approval (yellow)  
- DD Expiry tags: DD Expired (red), DD Expiring (yellow)

### Potential Future Uses
- **Contact**: Active, Inactive, VIP, Key Contact
- **Interaction**: Urgent, Follow-up Required, Completed
- **Project**: Active, On Hold, Completed, At Risk
- **Opportunity**: Open, In Progress, Won, Lost, Expired

## Implementation Steps for New Entity

1. Add `EntityTagModel` reference to your project
2. Create computed `Tags` property in your Model class: `public List<EntityTagModel>? Tags => CalculateConditionalTags();`
3. Implement `CalculateConditionalTags()` method with your entity's specific tag logic
4. Use `<app-entity-tags [tags]="entity.tags">` in your frontend templates
5. That's it! No configuration needed.