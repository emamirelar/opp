# Product & Service Search Enhancement - UX Design Document

## Overview

This document describes the enhanced hierarchical search UX for the "Add Product or Service" dialog in the Opportunity WHAT section. The enhancement makes it easy for users to search and select items at any level of the 5-level hierarchy while maintaining full transparency about the structure.

## Problem Statement

The original implementation had:
- A cascading dropdown approach (Level 0 → Level 1 → Level 2 → Level 3 → Level 4)
- A toggle-based "quick search" as a secondary feature
- Users had to understand the hierarchy to navigate effectively
- Difficult to find items when you don't know which level they're at

## Solution: Unified Hierarchical Search

### Core Design Principles

1. **Search-First Interface**: Make search the primary interaction method, not an afterthought
2. **Transparent Hierarchy**: Show the full hierarchical path in search results
3. **Visual Clarity**: Use color-coding and badges to indicate level depth
4. **Flexible Selection**: Allow selection at any level with clear indicators
5. **Preserve Browse Mode**: Keep cascading dropdowns for users who prefer structured navigation

## User Experience Flow

### 1. Primary Interface: **Search Mode**

When users open the dialog, they see:

```
┌──────────────────────────────────────────────────────┐
│  🔍 Search Products & Services    [Switch to Browse] │
├──────────────────────────────────────────────────────┤
│                                                       │
│  🔍 Unified Search                                   │
│  ┌─────────────────────────────────────────────┐   │
│  │ 🔍 Search by name, category, service line...│ × │
│  └─────────────────────────────────────────────┘   │
│  ℹ️ Search across all hierarchy levels             │
│                                                       │
│  📊 Search Results: 12                     Click to select
│  ┌─────────────────────────────────────────────────┐
│  │ ▸ Level 0: Service Category (2)                 │
│  │                                                   │
│  │  [L0] Project Management                         │
│  │  ├─ Project Management                          →│
│  │  │  Has Sub-levels                               │
│  │                                                   │
│  │ ▸ Level 1: Primary Service (4)                  │
│  │                                                   │
│  │  [L1] Infrastructure Development                 │
│  │  ├─ Project Management > Infrastructure Dev     →│
│  │  │  Most Specific Level  🏗 Infrastructure      │
│  │                                                   │
│  │ ▸ Level 2: Specific Service (3)                 │
│  │                                                   │
│  │  [L2] Road Construction                          │
│  │  ├─ Project Management > Infrastructure Dev >   │
│  │  │   Road Construction                          →│
│  │  │  Has Sub-levels  🏗 Infrastructure           │
│  │                                                   │
│  └─────────────────────────────────────────────────┘
│                                                       │
│  [Cancel]                                   [Add]    │
└──────────────────────────────────────────────────────┘
```

#### Key Features:

1. **Search Bar**: 
   - Always prominent at the top
   - Real-time search across all fields (name, levels, service line, definitions)
   - Minimum 2 characters to start searching

2. **Grouped Results**:
   - Results grouped by level depth (Level 0, Level 1, etc.)
   - Each group shows count of matches
   - Collapsible sections for better organization

3. **Rich Result Cards**:
   - **Level Badge** (L0-L4): Color-coded circle showing depth
     - L0: Blue
     - L1: Purple
     - L2: Pink
     - L3: Orange
     - L4: Green
   - **Item Name**: The most specific level name displayed prominently
   - **Breadcrumb Path**: Full hierarchy shown with color-coded chips
   - **Service Line Badge**: Blue chip with service line name
   - **Child Indicator**: 
     - "Has Sub-levels" badge if item has children
     - "Most Specific Level" badge if it's a terminal node

4. **Selection**:
   - Click anywhere on the result card to select
   - Chevron icon on the right indicates clickability
   - Selected item appears in confirmation panel below

### 2. Secondary Interface: **Browse Mode**

For users who prefer structured navigation:

```
┌──────────────────────────────────────────────────────┐
│  📚 Browse Products & Services   [Switch to Search]  │
├──────────────────────────────────────────────────────┤
│                                                       │
│  📍 Current Path:                                    │
│  [Project Management] → [Infrastructure] → [Roads]   │
│                                                       │
│  Level 0: Service Category                          │
│  ┌─────────────────────────────────────────────┐   │
│  │ Select service category...               ▾ │   │
│  └─────────────────────────────────────────────┘   │
│  ✓ Select at this level: Project Management        │
│                                                       │
│  Level 1: Primary Service                           │
│  ┌─────────────────────────────────────────────┐   │
│  │ Select primary service...                 ▾ │   │
│  └─────────────────────────────────────────────┘   │
│  ℹ️ Definition: Infrastructure development...       │
│  ✓ Select at this level: Infrastructure             │
│                                                       │
│  [Continue with Level 2, 3, 4...]                   │
│                                                       │
│  [Cancel]                                   [Add]    │
└──────────────────────────────────────────────────────┘
```

This preserves the original cascading dropdown functionality.

## Visual Design System

### Color Coding by Level

- **Level 0** (Service Category): Blue (#3B82F6)
  - Background: `bg-blue-100`, Text: `text-blue-700`
  
- **Level 1** (Primary Service): Purple (#9333EA)
  - Background: `bg-purple-100`, Text: `text-purple-700`
  
- **Level 2** (Specific Service): Pink (#DB2777)
  - Background: `bg-pink-100`, Text: `text-pink-700`
  
- **Level 3** (Detailed Service): Orange (#F97316)
  - Background: `bg-orange-100`, Text: `text-orange-700`
  
- **Level 4** (Most Specific): Green (#10B981)
  - Background: `bg-green-100`, Text: `text-green-700`

### Icons and Indicators

- 🔍 Search icon - Search functionality
- 📚 List icon - Browse mode
- ▸ Chevron right - Collapsible sections, clickable items
- [L0]-[L4] - Level badge with depth indicator
- 🔵 Has Sub-levels - Item has children (with sitemap icon)
- ✅ Most Specific Level - Terminal node (with check-circle icon)
- 🏗 Infrastructure - Service line badge (contextual icon)

## Technical Implementation

### New TypeScript Methods

1. **`performUnifiedSearch(query: string)`**: 
   - Searches across all Output fields
   - Sorts results by relevance (exact matches first, then by depth)
   
2. **`getGroupedSearchResults()`**: 
   - Groups search results by level depth
   - Returns Map<number, Output[]>

3. **`selectFromUnifiedSearch(output: Output)`**: 
   - Handles selection from search results
   - Auto-populates all level controls
   - Clears search state

4. **`toggleSearchMode()`**: 
   - Switches between 'search' and 'browse' modes
   - Clears search state when switching

5. **`getLevelDepth(output: Output)`**: 
   - Determines the depth (0-4) of an output

6. **`getDeepestLevel(output: Output)`**: 
   - Returns the most specific level name

7. **`getLevelLabel(depth: number)`**: 
   - Returns translated label for level

8. **`hasChildLevels(output: Output)`**: 
   - Checks if output has more specific sub-levels

### New Signals

```typescript
searchMode = signal<'search' | 'browse'>('search');
searchQuery = signal<string>('');
searchResults = signal<Output[]>([]);
```

### Search Algorithm

1. **Input Validation**: Minimum 2 characters
2. **Field Matching**: Searches across:
   - `name`, `level0-4`, `serviceLine`
   - `definitionLevel1-4`
3. **Ranking**:
   - Exact matches prioritized
   - Deeper levels (more specific) ranked higher
4. **Grouping**: Results grouped by level for clarity

## Translation Keys

Added to `en.json` (and should be replicated in es.json, fr.json, pt.json):

```json
{
  "button.switchToSearch": "Switch to Search",
  "button.switchToBrowse": "Switch to Browse",
  
  "label.unifiedSearch": "Unified Search",
  "label.searchResults": "Search Results",
  "label.level": "Level",
  "label.hasSublevels": "Has Sub-levels",
  "label.mostSpecificLevel": "Most Specific Level",
  
  "message.searchAcrossAllLevels": "Search across all hierarchy levels...",
  "message.clickToSelect": "Click to select",
  "message.startTypingToSearch": "Start typing to search across all levels",
  "message.searchMinimum2Chars": "Enter at least 2 characters to begin searching",
  "message.tryDifferentSearch": "Try different keywords or browse by category",
  
  "placeholder.searchAnyLevel": "Search by name, category, service line, or any keyword...",
  
  "title.searchProductsServices": "Search Products & Services",
  "title.browseProductsServices": "Browse Products & Services"
}
```

## User Benefits

### 1. **Faster Discovery**
- No need to understand hierarchy upfront
- Find items immediately by typing keywords
- See all matching items across all levels

### 2. **Better Context**
- Full hierarchical path shown in breadcrumb
- Level depth clearly indicated
- Service line and other metadata visible

### 3. **Informed Selection**
- Visual indicators show if item has children
- Can select parent categories or specific items
- Clear differentiation between levels

### 4. **Flexibility**
- Power users can use search
- Traditional users can use browse mode
- Both modes lead to the same result

## Example Scenarios

### Scenario 1: User knows what they want
1. Opens dialog → Search mode active
2. Types "road construction"
3. Sees results grouped by level
4. Sees "Road Construction" at Level 2 with breadcrumb: "Project Management > Infrastructure > Road Construction"
5. Clicks to select → Done

### Scenario 2: User is exploring
1. Opens dialog → Search mode active
2. Types "infrastructure"
3. Sees 15 results across multiple levels
4. Sees some items have "Has Sub-levels" badge
5. Can select broad "Infrastructure" category OR drill down to specific items
6. Makes informed choice based on badges and descriptions

### Scenario 3: User prefers structured approach
1. Opens dialog → Clicks "Switch to Browse"
2. Navigates through cascading dropdowns
3. Uses breadcrumb trail to track progress
4. Selects at appropriate level with "Select at this level" button

## Accessibility Considerations

- ✅ Keyboard navigation supported (tab through results)
- ✅ Screen reader friendly with semantic HTML
- ✅ ARIA labels for interactive elements
- ✅ High contrast color scheme
- ✅ Clear focus indicators

## Future Enhancements (Potential)

1. **Recent Selections**: Show recently selected items at top
2. **Favorites**: Allow users to mark frequently used items
3. **Synonyms**: Support common synonyms in search
4. **Filters**: Filter by service line, component flags
5. **AI Suggestions**: Pre-populate based on opportunity context

## Summary

This enhancement transforms the product/service selection from a hierarchical navigation challenge into an intuitive search experience while preserving the ability to browse when desired. Users can now:

- **Search freely** across all levels without knowing the structure
- **See context** with full hierarchical paths and visual indicators
- **Select confidently** knowing whether items have children or not
- **Switch modes** between search and browse as needed

The design makes the 5-level hierarchy transparent rather than a barrier, enabling faster and more confident selections.

