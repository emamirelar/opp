# Opportunity View - Loading UX Visual Mockups

**Date**: January 2025  
**Purpose**: Visual representations of loading progress UX options

---

## Option 1: Top-Fixed Progress Strip ⭐ RECOMMENDED

### Desktop View

```
┌──────────────────────────────────────────────────────────────────────────────┐
│  [Banner Image with Opportunity Photo]                                      │
│                                                                              │
│  ← Opportunity Name: Partnership Development Initiative    [Active][Stage]  │
│  ID: 12345 | Manager: John Smith | Org Unit: Global Programs               │
├──────────────────────────────────────────────────────────────────────────────┤
│  [📊 Analysis] [📄 Overview] [💼 What] [💡 Why] [👥 Who] [🏢 Team] [More▼]  │
├──────────────────────────────────────────────────────────────────────────────┤
│  ████████████████████░░░░░░░░  ⚡ Loading: AI Recommendations (6 of 10)  75%│ ← Progress Strip
├──────────────────────────────────────────────────────────────────────────────┤
│                                                                              │
│  [Documents Panel]   ┌──────────────────────────────────────────────────┐  │
│  • Document 1        │ 📊 Analysis                                      │  │
│  • Document 2        │ Total Funding: $1.2M | Partners: 5 | SDGs: 3   │  │
│  • Document 3        │                                                  │  │
│                      │ Quick Stats loaded ✓                            │  │
│                      └──────────────────────────────────────────────────┘  │
│                      ┌──────────────────────────────────────────────────┐  │
│                      │ 📄 Overview                                      │  │
│                      │ [Content fully loaded]                           │  │
│                      └──────────────────────────────────────────────────┘  │
│                      ┌──────────────────────────────────────────────────┐  │
│                      │ 💼 What                                          │  │
│                      │ [Content fully loaded]                           │  │
│                      └──────────────────────────────────────────────────┘  │
│                      ┌──────────────────────────────────────────────────┐  │
│                      │ 🎯 Risks & Recommendations                       │  │
│                      │                                                  │  │
│                      │ ▓▓▓▓▓▓▓▓░░░░░░░░ Loading AI Recommendations...  │  │ ← Section Skeleton
│                      │                                                  │  │
│                      │ ████████████████ ░░░░░░░░░░░░░░░░              │  │
│                      │ ████████████ ░░░░░░░░░░░░░░░░░░░░              │  │
│                      └──────────────────────────────────────────────────┘  │
│                                                                              │
└──────────────────────────────────────────────────────────────────────────────┘
```

### Mobile View

```
┌────────────────────────────────┐
│ [Banner Image]                 │
│                                │
│ ← Opportunity Name             │
│ [Active] [Stage]               │
├────────────────────────────────┤
│ [Section Dropdown ▼]           │
├────────────────────────────────┤
│ ███████████░░░░  ⚡ 75%        │ ← Compact Progress
├────────────────────────────────┤
│                                │
│ 📊 Analysis                    │
│ $1.2M | 5 Partners             │
│                                │
│ 📄 Overview                    │
│ [Content]                      │
│                                │
│ 🎯 Risks                       │
│ ▓▓▓░░░ Loading...             │ ← Skeleton
│                                │
└────────────────────────────────┘
```

### Progress Strip States

#### Loading State (1 of 10 sections)
```
┌──────────────────────────────────────────────────────────────────┐
│ ██░░░░░░░░░░░░░░░░░░  ⚡ Loading: Opportunity Data (1 of 10)  10%│
└──────────────────────────────────────────────────────────────────┘
```

#### Mid-Loading State (6 of 10 sections)
```
┌──────────────────────────────────────────────────────────────────┐
│ ████████████░░░░░░░░  ⚡ Loading: AI Recommendations (6 of 10) 60%│
└──────────────────────────────────────────────────────────────────┘
```

#### Almost Complete (9 of 10 sections)
```
┌──────────────────────────────────────────────────────────────────┐
│ ██████████████████░░  ⚡ Loading: Documents (9 of 10)         90%│
└──────────────────────────────────────────────────────────────────┘
```

#### Complete (auto-hides after 2 seconds)
```
┌──────────────────────────────────────────────────────────────────┐
│ ████████████████████  ✓ All data loaded successfully          100%│
└──────────────────────────────────────────────────────────────────┘
```

---

## Option 2: Floating Toast Indicator

### Desktop View

```
┌──────────────────────────────────────────────────────────────────────────────┐
│  [Banner Image]                                                              │
│                                                                              │
│  ← Opportunity Name                                [Active][Stage]           │
├──────────────────────────────────────────────────────────────────────────────┤
│  [📊 Analysis] [📄 Overview] [💼 What] [More▼]              ┌──────────────┐│
│                                                               │ 🔄 Loading   ││
│                                                               │ ████████░░   ││ ← Toast
│  [Documents]     ┌──────────────────────────────────────┐    │ 65%          ││
│  • Doc 1         │ 📊 Analysis                          │    │              ││
│  • Doc 2         │ [Content]                            │    │ AI Recs...   ││
│                  └──────────────────────────────────────┘    │ (6/10)     X ││
│                  ┌──────────────────────────────────────┐    └──────────────┘│
│                  │ 📄 Overview                          │                    │
│                  │ [Content]                            │                    │
│                  └──────────────────────────────────────┘                    │
│                                                                              │
└──────────────────────────────────────────────────────────────────────────────┘
```

### Toast States

#### Initial Toast
```
┌──────────────────────────┐
│ 🔄 Loading              X│
│ ████░░░░░░░░░░░░  20%   │
│                          │
│ Loading opportunity...   │
│ (2 of 10 sections)       │
└──────────────────────────┘
```

#### Mid-Loading Toast
```
┌──────────────────────────┐
│ 🔄 Loading              X│
│ ███████████░░░░░  65%   │
│                          │
│ Loading AI Recs...       │
│ (6 of 10 sections)       │
└──────────────────────────┘
```

#### Completion Toast (auto-dismisses)
```
┌──────────────────────────┐
│ ✓ Complete              X│
│ ████████████████  100%  │
│                          │
│ All data loaded!         │
└──────────────────────────┘
```

---

## Option 3: Section-Level Skeleton Loaders

### Desktop View with Multiple Skeleton States

```
┌──────────────────────────────────────────────────────────────────────────────┐
│  [Banner Image]                                                              │
│  ← Opportunity Name                                [Active][Stage]           │
├──────────────────────────────────────────────────────────────────────────────┤
│  [📊 Analysis] [📄 Overview] [💼 What] [💡 Why] [👥 Who] [More▼]            │
├──────────────────────────────────────────────────────────────────────────────┤
│                                                                              │
│  [Docs]          ┌─────────────────────────────────────────────────────┐   │
│  • Doc 1         │ 📊 Analysis                                  ✓ Loaded│   │
│  • Doc 2         │ Total Funding: $1.2M | Partners: 5 | SDGs: 3       │   │
│                  │ [Fully rendered content with charts and data]       │   │
│                  └─────────────────────────────────────────────────────┘   │
│                  ┌─────────────────────────────────────────────────────┐   │
│                  │ 📄 Overview                                  ✓ Loaded│   │
│                  │ [Fully rendered content]                            │   │
│                  └─────────────────────────────────────────────────────┘   │
│                  ┌─────────────────────────────────────────────────────┐   │
│                  │ 🎯 Risks & Recommendations               🔄 Loading │   │
│                  │                                                     │   │
│                  │ ▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓░░░░░░░░  [shimmer animation]     │   │
│                  │ ▓▓▓▓▓▓▓▓▓▓▓▓░░░░░░░░░░░░  Loading AI insights...  │   │
│                  │                                                     │   │
│                  │ ┌──────────────┐ ┌──────────────┐                 │   │
│                  │ │ ▓▓▓▓▓▓▓▓░░░░ │ │ ▓▓▓▓▓▓▓▓░░░░ │                 │   │
│                  │ │ ▓▓▓▓▓░░░░░░░ │ │ ▓▓▓▓▓░░░░░░░ │                 │   │
│                  │ └──────────────┘ └──────────────┘                 │   │
│                  └─────────────────────────────────────────────────────┘   │
│                  ┌─────────────────────────────────────────────────────┐   │
│                  │ 🔗 Related Items                         🔄 Loading │   │
│                  │                                                     │   │
│                  │ ░░░░░░░░░░░░░░░░░░░░  [subtle skeleton loading]   │   │
│                  │ ░░░░░░░░░░░░░░░░                                   │   │
│                  └─────────────────────────────────────────────────────┘   │
│                  ┌─────────────────────────────────────────────────────┐   │
│                  │ 💬 Comments                                 ✓ Ready │   │
│                  │ [Fully rendered comment thread]                     │   │
│                  └─────────────────────────────────────────────────────┘   │
│                                                                              │
└──────────────────────────────────────────────────────────────────────────────┘
```

### Skeleton Patterns

#### Card Skeleton (for Risk Cards, Recommendation Cards)
```
┌─────────────────────────────────┐
│ ▓▓▓▓▓▓▓▓▓▓▓▓░░░░░░  [shimmer]  │ ← Title skeleton
│                                 │
│ ▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓░░░░  │ ← Text line 1
│ ▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓░░░░░░░░░░  │ ← Text line 2
│ ▓▓▓▓▓▓▓▓▓▓▓▓▓▓░░░░░░░░░░░░░░  │ ← Text line 3
│                                 │
│ ▓▓▓▓▓▓░░░  ▓▓▓▓▓▓░░░           │ ← Buttons skeleton
└─────────────────────────────────┘
```

#### List Skeleton (for Document List, Related Items)
```
┌─────────────────────────────────┐
│ ○ ▓▓▓▓▓▓▓▓▓▓▓▓░░░░░░           │ ← List item 1
│ ○ ▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓░░░           │ ← List item 2
│ ○ ▓▓▓▓▓▓▓▓░░░░░░░░░░           │ ← List item 3
│ ○ ▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓░░         │ ← List item 4
└─────────────────────────────────┘
```

#### Chart Skeleton (for Analysis Section)
```
┌─────────────────────────────────┐
│ ▓▓▓▓▓▓▓▓▓▓░░░                  │ ← Chart title
│                                 │
│  █                              │
│  █         █                    │
│  █         █         █          │ ← Bar chart skeleton
│  █         █         █      █   │
│ ─┴─────────┴─────────┴──────┴─ │
│ ▓░  ▓░░  ▓░░░  ▓░░            │ ← Legend skeleton
└─────────────────────────────────┘
```

---

## Recommended Hybrid Approach: Combined View

### Complete Loading Experience

```
STAGE 1: Initial Load (0-1 second)
┌──────────────────────────────────────────────────────────────────┐
│ [Banner Image]                                                   │
│ ← Opportunity Name                          [Active][Stage]      │
├──────────────────────────────────────────────────────────────────┤
│ [Section Chips]                                                  │
├──────────────────────────────────────────────────────────────────┤
│ ██░░░░░░░░░░░░░░░░  ⚡ Loading: Opportunity Data (1 of 10)   10%│ ← Progress Bar
├──────────────────────────────────────────────────────────────────┤
│                                                                  │
│ [Docs]   ┌─────────────────────────────────────────────────┐   │
│          │ 📊 Analysis                         🔄 Loading   │   │
│          │ ▓▓▓▓▓▓▓▓░░░░ Loading stats...                   │   │ ← Skeleton
│          └─────────────────────────────────────────────────┘   │
└──────────────────────────────────────────────────────────────────┘

STAGE 2: Mid-Loading (2-3 seconds)
┌──────────────────────────────────────────────────────────────────┐
│ [Banner Image]                                                   │
│ ← Opportunity Name                          [Active][Stage]      │
├──────────────────────────────────────────────────────────────────┤
│ [Section Chips]                                                  │
├──────────────────────────────────────────────────────────────────┤
│ ████████████░░░░░░  ⚡ Loading: AI Recommendations (6 of 10) 60%│ ← Progress Bar
├──────────────────────────────────────────────────────────────────┤
│                                                                  │
│ [Docs]   ┌─────────────────────────────────────────────────┐   │
│          │ 📊 Analysis                              ✓ Ready│   │
│          │ $1.2M | 5 Partners | 3 SDGs                    │   │ ← Loaded
│          └─────────────────────────────────────────────────┘   │
│          ┌─────────────────────────────────────────────────┐   │
│          │ 📄 Overview                              ✓ Ready│   │
│          │ [Full content visible]                          │   │ ← Loaded
│          └─────────────────────────────────────────────────┘   │
│          ┌─────────────────────────────────────────────────┐   │
│          │ 🎯 Risks                             🔄 Loading │   │
│          │ ▓▓▓▓▓▓▓░░░ Loading AI recommendations...       │   │ ← Skeleton
│          └─────────────────────────────────────────────────┘   │
└──────────────────────────────────────────────────────────────────┘

STAGE 3: Complete (4-5 seconds, then auto-hide progress bar)
┌──────────────────────────────────────────────────────────────────┐
│ [Banner Image]                                                   │
│ ← Opportunity Name                          [Active][Stage]      │
├──────────────────────────────────────────────────────────────────┤
│ [Section Chips]                                                  │
├──────────────────────────────────────────────────────────────────┤
│ ████████████████████  ✓ All data loaded successfully       100%│ ← Will fade out
├──────────────────────────────────────────────────────────────────┤
│                                                                  │
│ [Docs]   ┌─────────────────────────────────────────────────┐   │
│          │ 📊 Analysis                              ✓ Ready│   │
│          │ [All stats and charts fully rendered]           │   │ ← All Loaded
│          └─────────────────────────────────────────────────┘   │
│          ┌─────────────────────────────────────────────────┐   │
│          │ 📄 Overview                              ✓ Ready│   │
│          │ [Full content]                                  │   │ ← All Loaded
│          └─────────────────────────────────────────────────┘   │
│          ┌─────────────────────────────────────────────────┐   │
│          │ 🎯 Risks & Recommendations               ✓ Ready│   │
│          │ [All AI recommendations loaded and displayed]   │   │ ← All Loaded
│          └─────────────────────────────────────────────────┘   │
│          ┌─────────────────────────────────────────────────┐   │
│          │ 🔗 Related Items                         ✓ Ready│   │
│          │ [Source interactions displayed]                 │   │ ← All Loaded
│          └─────────────────────────────────────────────────┘   │
└──────────────────────────────────────────────────────────────────┘
```

---

## Color Coding & Visual Language

### Status Indicators

#### Loading State
- **Color**: Blue (`#00669a` - UNOPS Primary)
- **Icon**: Spinning spinner (`pi-spin pi-spinner`)
- **Animation**: Smooth rotation, shimmer effect

#### Completed State
- **Color**: Green (`#4caf50`)
- **Icon**: Check circle (`pi-check-circle`)
- **Animation**: Fade in with scale

#### Error State
- **Color**: Red (`#ef4444`)
- **Icon**: Times circle (`pi-times-circle`)
- **Animation**: Shake effect

#### Pending State
- **Color**: Gray (`#9ca3af`)
- **Icon**: Circle outline (`pi-circle`)
- **Animation**: None

### Progress Bar Colors

```
Gradient Fill:
┌─────────────────────────────────────┐
│ ████████████░░░░░░░░░░░░░░░░░░░░░░ │
│ ^Blue^     ^Light Gray Background^  │
│ #00669a    #e5e7eb                  │
└─────────────────────────────────────┘
```

### Skeleton Loader Animation

```
Time: 0s          0.5s          1.0s          1.5s
┌───────────┐   ┌───────────┐   ┌───────────┐   ┌───────────┐
│███░░░░░░░░│ → │░██░░░░░░░░│ → │░░██░░░░░░░│ → │░░░██░░░░░░│
└───────────┘   └───────────┘   └───────────┘   └───────────┘
   ^Shimmer effect moving left to right (continuous loop)^
   
Colors:
- Base: #f0f0f0
- Shimmer: #e0e0e0
- Speed: 1.5s per cycle
```

---

## Responsive Behavior

### Desktop (1200px+)
- Full progress bar with detailed message
- Percentage shown on right
- All section chips visible
- Skeleton loaders show full detail

### Tablet (768px - 1199px)
- Condensed progress bar
- Abbreviated message
- Some section chips in "More" dropdown
- Skeleton loaders slightly simplified

### Mobile (< 768px)
- Minimal progress bar (thin line only)
- Percentage only
- All sections in dropdown
- Minimal skeleton loaders
- Documents panel collapsible

---

## Accessibility Features

### Screen Reader Announcements

```html
<div role="status" aria-live="polite" aria-atomic="true">
  Loading: AI Recommendations (6 of 10 sections complete, 60%)
</div>
```

### Keyboard Navigation

- Progress bar not focusable (visual only)
- Dismiss button (if present) is keyboard accessible
- Sections remain keyboard navigable during loading
- Loading skeletons don't trap focus

### Reduced Motion Support

```css
@media (prefers-reduced-motion: reduce) {
  /* Disable shimmer animations */
  .skeleton-loader {
    animation: none;
    background: solid #f0f0f0;
  }
  
  /* Disable spinner rotation */
  .pi-spinner {
    animation: none;
  }
}
```

---

## Performance Considerations

### Animation Performance
- Use `transform` and `opacity` for animations (GPU-accelerated)
- Avoid `width` or `height` animations on progress bar
- Use `will-change: transform` sparingly

### Rendering Optimization
- Skeleton loaders use CSS only (no images)
- Progress bar updates max every 100ms (throttled)
- Component change detection on Push mode
- Lazy load sections below fold

---

## Summary Comparison Table

| Feature | Option 1: Progress Strip | Option 2: Toast | Option 3: Skeletons |
|---------|-------------------------|-----------------|---------------------|
| **Visibility** | ⭐⭐⭐⭐⭐ Always visible | ⭐⭐⭐ May be overlooked | ⭐⭐⭐⭐ Per-section clarity |
| **Intrusiveness** | ⭐⭐⭐⭐⭐ Minimal | ⭐⭐⭐⭐⭐ Dismissible | ⭐⭐⭐⭐ Non-blocking |
| **Information** | ⭐⭐⭐⭐ Overall progress | ⭐⭐⭐ Basic progress | ⭐⭐⭐⭐⭐ Specific sections |
| **Mobile-Friendly** | ⭐⭐⭐⭐⭐ Responsive | ⭐⭐⭐ Takes space | ⭐⭐⭐⭐ Adapts well |
| **Implementation** | ⭐⭐⭐⭐ Moderate effort | ⭐⭐⭐⭐⭐ Easy | ⭐⭐⭐ More effort |
| **User Feedback** | ⭐⭐⭐⭐⭐ Clear & modern | ⭐⭐⭐⭐ Familiar pattern | ⭐⭐⭐⭐⭐ Expected UX |

**🏆 Recommended**: **Hybrid Approach** combining Option 1 (Progress Strip) + Option 3 (Skeletons)
- Best overall user experience
- Clear global and section-level progress
- Professional, modern interface
- Non-intrusive but informative

