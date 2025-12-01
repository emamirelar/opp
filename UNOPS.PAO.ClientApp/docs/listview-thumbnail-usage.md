# Listview Thumbnail Column Type

## Overview

The `thumbnail` column type allows displaying square or rectangular images in the listview card component. This is ideal for opportunity thumbnails, project logos, organization badges, and other non-circular images.

## Key Differences from Avatar Type

| Feature | Avatar Type | Thumbnail Type |
|---------|-------------|----------------|
| Shape | Always circular | Square with configurable border-radius |
| Use Case | People, users, contacts | Logos, banners, projects, opportunities |
| Sizing | PrimeNG sizes (normal, large) | Custom pixel sizes (32px-128px) |
| Border | Built-in PrimeNG styling | Configurable border |
| Fallback | Default placeholder images | Custom fallback URL |

## Configuration Properties

### `thumbnailSize`
Size of the thumbnail (width and height).

**Options:**
- `'32px'` - Extra small (w-8 h-8)
- `'40px'` - Small (w-10 h-10)
- `'48px'` - **Default** - Medium (w-12 h-12)
- `'56px'` - Medium-Large (w-14 h-14)
- `'64px'` - Large (w-16 h-16)
- `'80px'` - Extra Large (w-20 h-20)
- `'96px'` - 2X Large (w-24 h-24)
- `'128px'` - 3X Large (w-32 h-32)

### `thumbnailShape`
Border radius style for the thumbnail.

**Options:**
- `'square'` - No border radius (sharp corners)
- `'rounded'` - Small border radius (4px)
- `'rounded-lg'` - **Default** - Medium border radius (8px)
- `'rounded-xl'` - Large border radius (12px)

### `thumbnailBorder`
Whether to show a border around the thumbnail.

**Type:** `boolean`  
**Default:** `true`  
**Border Style:** `border border-unops-neutral-300`

### `thumbnailFallback`
Fallback image URL when the field value is null or empty.

**Type:** `string`  
**Example:** `'assets/images/opportunity-placeholder.png'`

## Usage Examples

### Example 1: Basic Opportunity Thumbnail

```typescript
const opportunityColumns: ListViewColumn[] = [
  {
    field: 'opportunityThumbnail',
    label: 'Thumbnail',
    type: 'thumbnail',
    sortable: false,
    thumbnailSize: '48px',
    thumbnailShape: 'rounded-lg',
    thumbnailBorder: true
  },
  {
    field: 'name',
    label: 'Opportunity Name',
    type: 'text',
    sortable: true
  },
  // ... other columns
];
```

### Example 2: Large Project Logo with Fallback

```typescript
const projectColumns: ListViewColumn[] = [
  {
    field: 'projectLogoUrl',
    label: 'Logo',
    type: 'thumbnail',
    sortable: false,
    thumbnailSize: '64px',
    thumbnailShape: 'rounded-xl',
    thumbnailBorder: false,
    thumbnailFallback: 'assets/images/project-default.png'
  },
  // ... other columns
];
```

### Example 3: Small Organization Badge

```typescript
const organizationColumns: ListViewColumn[] = [
  {
    field: 'organizationBadge',
    label: 'Badge',
    type: 'thumbnail',
    sortable: false,
    thumbnailSize: '32px',
    thumbnailShape: 'square',
    thumbnailBorder: true
  },
  // ... other columns
];
```

### Example 4: Full Configuration

```typescript
{
  field: 'opportunityThumbnail',
  label: 'Opportunity Logo',
  type: 'thumbnail',
  sortable: false,
  thumbnailSize: '48px',           // Medium size thumbnail
  thumbnailShape: 'rounded-lg',    // Rounded corners
  thumbnailBorder: true,           // Show border
  thumbnailFallback: 'assets/images/opportunity-placeholder.png', // Fallback image
  helperText: 'AI-generated opportunity logo'
}
```

## Display Behavior

### In Card View
1. **With Thumbnail Column**: The thumbnail displays in the left avatar/icon section of the card
2. **Priority Order**: Thumbnail > Avatar > Interaction Icon
3. **Field 1 Handling**: If Field 1 is a thumbnail column, the text content (entity name) displays as the title, not the thumbnail
4. **Responsive**: Thumbnails show based on `shouldShowAvatar()` computed property (hidden on very small cards)

### In Template Fields
Thumbnails can also appear inline within field content using the `fieldTemplate`:
- Displays the thumbnail image
- Optional: Shows entity name next to thumbnail (unless `context === 'thumbnail-only'`)
- Falls back to `thumbnailFallback` image if field value is empty

## Technical Implementation

### Model Changes
- Added `'thumbnail'` to column type union in `listview.model.ts`
- Added thumbnail-specific configuration properties

### Component Changes
- Added `thumbnailColumn` computed property in `listview-card.component.ts`
- Added `getThumbnailClasses()` helper method for CSS class generation
- Updated `shouldShowInteractionAvatar()` to respect thumbnail priority

### Template Changes
- Added `@case ('thumbnail')` handler in fieldTemplate
- Updated avatar/icon section to prioritize and display thumbnail columns
- Added thumbnail handling in field1 title display

## Best Practices

1. **Use Appropriate Sizes**: 
   - List views: 48px-64px
   - Detail cards: 80px-96px
   - Grid galleries: 96px-128px

2. **Match Content Type**:
   - Square logos: `shape: 'square'` or `'rounded'`
   - Photos: `shape: 'rounded-lg'` or `'rounded-xl'`
   - Brand marks: `shape: 'square'`

3. **Provide Fallbacks**:
   - Always specify `thumbnailFallback` for better UX
   - Use entity-specific placeholder images

4. **Consider Performance**:
   - Use appropriately sized source images
   - Consider lazy loading for large lists
   - Store images as data URIs or CDN URLs

## Integration with Opportunity Images

For opportunities with AI-generated images:

```typescript
{
  field: 'opportunityThumbnail',
  label: 'Logo',
  type: 'thumbnail',
  sortable: false,
  thumbnailSize: '48px',
  thumbnailShape: 'rounded-lg',
  thumbnailBorder: true,
  thumbnailFallback: 'assets/images/opportunity-placeholder.png',
  helperText: 'AI-generated opportunity logo'
}
```

This configuration will:
- Display the 1024x1024 AI-generated logo at 48x48px
- Show rounded corners matching the design system
- Include a subtle border for definition
- Fall back to placeholder if image generation is pending
- Scale perfectly from the source 1024x1024 resolution

