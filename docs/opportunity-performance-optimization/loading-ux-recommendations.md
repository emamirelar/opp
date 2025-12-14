# Opportunity View - Loading UX Optimization Recommendations

**Date**: January 2025  
**Component**: `opportunity-view.component.ts`  
**Objective**: Improve user experience during data loading with clear visual feedback and optimized loading sequence

---

## Executive Summary

The Opportunity View component loads data from multiple AI-powered backend services asynchronously. Currently, there's no unified indication of what's loading or what remains. This document proposes a comprehensive solution with:

1. **Sequential section loading** (top to bottom as displayed)
2. **Non-intrusive progress indication** (subtle but informative)
3. **Skeleton loaders** for individual sections
4. **Smart loading orchestration** to balance performance and UX

---

## Current State Analysis

### Data Loading Sources

```typescript
// Parent Component (_loadRecordDetails)
1. Opportunity entity data (immediate)
2. AI insights & suggestions (immediate, single API call)
3. Banner image generation (background, non-blocking)

// Child Sections (independent API calls)
Analysis Section:   Insights from parent ✅ (optimized)
Overview Section:   No loading (uses parent data)
What Section:       Framework status, AI recommendations
Why Section:        Targets, indicators
Who Section:        No loading (uses parent data)
Team Section:       No loading (uses parent data)
Where Section:      No loading (uses parent data)
When Section:       No loading (uses parent data)
DST Section:        Risks, Recommendations, Similar Opportunities, Similar Projects, Relevant People (5 staggered calls)
Related Section:    Source interactions
Collaboration:      Comments (handled by comment component)
Statement Section:  No loading (uses parent data)
Documents Panel:    Document list, upload status
```

### Loading Timing

```
0ms     → Opportunity data starts
0ms     → Insights API call starts
0ms     → DST Risks starts
500ms   → DST Recommendations starts
1000ms  → DST Similar Opportunities starts
1500ms  → DST Similar Projects starts
2000ms  → DST Relevant People starts
(varies) → Other sections load as needed
```

### Current Problems

1. ❌ **No unified progress indication** - User doesn't know overall loading status
2. ❌ **Invisible staggered loading** - Intentional delays are hidden from user
3. ❌ **Out-of-order loading** - Sections may complete loading in different order than displayed
4. ❌ **No clear completion signal** - User doesn't know when all data is loaded
5. ❌ **Abrupt content appearance** - Sections "pop in" without warning

---

## Recommended Solution: Progressive Loading Dashboard

### Design Principles

1. **Non-Intrusive** - Don't block the user or overwhelm with information
2. **Informative** - User should know what's loading and what's ready
3. **Predictable** - Loading should occur in visual order (top to bottom)
4. **Responsive** - Fast sections shouldn't wait for slow sections
5. **Professional** - Maintain UNOPS brand standards

### Implementation Strategy

#### Option 1: Top-Fixed Progress Strip (Recommended)

**Visual Design:**
```
┌─────────────────────────────────────────────────────────┐
│ [Banner Image]                                          │
│ Opportunity Name - Status - Stage                       │
├─────────────────────────────────────────────────────────┤
│ ⚡ Loading: AI Recommendations (4 of 8 sections ready) │ ← Subtle progress bar
├─────────────────────────────────────────────────────────┤
│ [Section Navigation Chips]                              │
└─────────────────────────────────────────────────────────┘
```

**Features:**
- Ultra-thin progress bar (2px height) below section navigation
- Minimal text indicator: "Loading... (5 of 8 complete)"
- Auto-hides after 2 seconds of completion
- Color-coded: Blue (loading), Green (complete)
- Doesn't push content down (overlay style)

**Advantages:**
- ✅ Always visible during scroll
- ✅ Non-intrusive (single line)
- ✅ Clear completion count
- ✅ Auto-dismisses when done
- ✅ Doesn't impact layout

#### Option 2: Floating Toast Indicator

**Visual Design:**
```
                            ┌──────────────────────────────┐
                            │ 🔄 Loading Insights...       │
                            │ █████████░░░░░  65%          │
                            └──────────────────────────────┘
                                    (top-right corner)
```

**Features:**
- PrimeNG Toast-style notification
- Shows current loading section
- Progress percentage
- Auto-updates as sections complete
- Dismissible by user

**Advantages:**
- ✅ Familiar toast pattern
- ✅ Completely non-intrusive
- ✅ User can dismiss if desired
- ✅ No layout impact

**Disadvantages:**
- ⚠️ May be overlooked by users
- ⚠️ Limited space for detail

#### Option 3: Section-Level Skeleton Loaders

**Visual Design:**
```
┌────────────────────────────────────────────────────┐
│ 📊 Analysis                                  [Edit]│
├────────────────────────────────────────────────────┤
│ ▓▓▓▓▓▓▓▓░░░░░░░░  Loading insights...            │
│                                                    │
│ ████████████████ ░░░░░░░░░░░░░░░░                │
│ ████████████ ░░░░░░░░░░░░░░░░░░░░                │
│ ████████████████████ ░░░░░░░░░░                  │
└────────────────────────────────────────────────────┘
```

**Features:**
- Each section shows skeleton/shimmer while loading
- Sections with data show immediately
- Sections waiting for data show loading state
- No overall progress indicator needed

**Advantages:**
- ✅ Per-section granularity
- ✅ User sees what's loading where
- ✅ Modern UX pattern
- ✅ No central indicator needed

**Disadvantages:**
- ⚠️ May feel "busy" with many skeletons
- ⚠️ Requires implementation in each section

---

## Recommended Implementation (Hybrid Approach)

**Combine the best of all three options:**

### 1. Top-Fixed Subtle Progress Strip (Primary)

```typescript
// In opportunity-view.component.ts

// Loading state tracking
readonly loadingProgress = signal<LoadingProgress>({
  total: 8,
  completed: 0,
  currentSection: '',
  sections: {
    opportunity: { status: 'pending', label: 'Opportunity Data' },
    insights: { status: 'pending', label: 'AI Insights' },
    analysis: { status: 'pending', label: 'Analysis' },
    dstRisks: { status: 'pending', label: 'Risk Assessment' },
    dstRecommendations: { status: 'pending', label: 'Recommendations' },
    dstSimilar: { status: 'pending', label: 'Similar Opportunities' },
    relatedItems: { status: 'pending', label: 'Related Items' },
    documents: { status: 'pending', label: 'Documents' }
  }
});

// Computed progress percentage
readonly progressPercentage = computed(() => {
  const progress = this.loadingProgress();
  return Math.round((progress.completed / progress.total) * 100);
});

// Computed progress message
readonly progressMessage = computed(() => {
  const progress = this.loadingProgress();
  if (progress.completed === progress.total) {
    return this.translateService.instant('message.allDataLoaded');
  }
  return this.translateService.instant('message.loadingProgress', {
    current: progress.completed,
    total: progress.total,
    section: progress.currentSection
  });
});

// Show progress bar only while loading
readonly showProgressBar = computed(() => {
  return this.loadingProgress().completed < this.loadingProgress().total;
});
```

### 2. Sequential Loading Orchestration

```typescript
/**
 * Orchestrated loading sequence - triggers sections in visual order (top to bottom)
 */
private _loadRecordDetails(targetSection?: string) {
  this.loading.set(true);
  
  // Reset progress
  this.updateLoadingProgress('opportunity', 'loading', 'Opportunity Data');
  
  // STEP 1: Load main opportunity data
  this.opportunityService.getOpportunityById(+this.recordId).subscribe({
    next: (data: Opportunity) => {
      this.opportunity.set(data);
      this.loading.set(false);
      this.updateLoadingProgress('opportunity', 'completed');
      
      // STEP 2: Load insights (required by Analysis section)
      this.updateLoadingProgress('insights', 'loading', 'AI Insights');
      this._loadInsights();
      
      // STEP 3: Generate banner images (background, non-blocking)
      if (data.name && data.description && !data.opportunityBannerImage) {
        this._generateBannerImages(data.id);
      }
      
      // STEP 4: Trigger section data loading in visual order
      this._orchestrateSectionLoading();
      
      // Handle initial scroll if needed
      if (this.shouldScrollAfterDataLoad && targetSection && this.isValidSection(targetSection)) {
        this.pendingScrollTarget = targetSection;
        this.shouldScrollAfterDataLoad = false;
        this.waitForContentAndScroll();
      } else {
        this.isInitialLoad = false;
      }
    },
    error: (error) => {
      console.error('Error loading opportunity details:', error);
      this.loading.set(false);
      this.updateLoadingProgress('opportunity', 'error');
      this.feedbackDialogService.showErrorToast({
        detail: this.translateService.instant('message.opportunity.loadFailed'),
        summary: this.translateService.instant('message.error'),
      });
    },
  });
}

/**
 * Orchestrate section loading in visual order (top to bottom)
 * Uses sequential delays to load sections as user scrolls down
 */
private _orchestrateSectionLoading(): void {
  // Analysis section (uses insights loaded in step 2) - immediate
  setTimeout(() => {
    this.updateLoadingProgress('analysis', 'completed', 'Analysis');
  }, 100);
  
  // DST Section - Risks (immediate)
  setTimeout(() => {
    this.updateLoadingProgress('dstRisks', 'loading', 'Risk Assessment');
    // DST component will load risks immediately
  }, 200);
  
  // DST Section - Recommendations (+500ms)
  setTimeout(() => {
    this.updateLoadingProgress('dstRecommendations', 'loading', 'AI Recommendations');
    // DST component will load recommendations
  }, 700);
  
  // DST Section - Similar Items (+1000ms)
  setTimeout(() => {
    this.updateLoadingProgress('dstSimilar', 'loading', 'Similar Opportunities');
    // DST component will load similar opportunities and projects
  }, 1200);
  
  // Related Items Section
  setTimeout(() => {
    this.updateLoadingProgress('relatedItems', 'loading', 'Related Items');
  }, 1700);
  
  // Documents Panel
  setTimeout(() => {
    this.updateLoadingProgress('documents', 'loading', 'Documents');
  }, 2000);
}

/**
 * Update loading progress for a section
 */
private updateLoadingProgress(
  sectionKey: keyof LoadingProgress['sections'],
  status: 'pending' | 'loading' | 'completed' | 'error',
  label?: string
): void {
  this.loadingProgress.update(progress => {
    const updatedSections = { ...progress.sections };
    updatedSections[sectionKey] = {
      ...updatedSections[sectionKey],
      status,
      label: label || updatedSections[sectionKey].label
    };
    
    const completed = Object.values(updatedSections).filter(s => s.status === 'completed').length;
    const currentLoadingSection = Object.values(updatedSections).find(s => s.status === 'loading');
    
    return {
      ...progress,
      sections: updatedSections,
      completed,
      currentSection: currentLoadingSection?.label || ''
    };
  });
}
```

### 3. Progress UI Component

```html
<!-- opportunity-view.component.html -->

<!-- Loading Progress Strip (Fixed below section navigation) -->
@if (showProgressBar()) {
  <div 
    class="loading-progress-strip sticky top-0 z-30 bg-unops-surface-primary border-b border-unops-neutral-200 transition-all duration-300"
  >
    <div class="flex items-center justify-between px-unops-md md:px-unops-2xl py-2">
      <!-- Progress Bar -->
      <div class="flex-1 mr-4">
        <div class="w-full bg-unops-neutral-200 rounded-full h-1.5 overflow-hidden">
          <div 
            class="bg-unops-primary h-1.5 rounded-full transition-all duration-500 ease-out"
            [style.width.%]="progressPercentage()"
          ></div>
        </div>
      </div>
      
      <!-- Progress Text -->
      <div class="flex items-center gap-3 text-sm text-unops-neutral-700">
        <span class="flex items-center gap-2">
          <i class="pi pi-spin pi-spinner text-unops-primary"></i>
          <span class="font-medium">{{ progressMessage() }}</span>
        </span>
        <span class="text-unops-neutral-500">
          {{ progressPercentage() }}%
        </span>
      </div>
    </div>
  </div>
}
```

### 4. Section-Level Skeleton Loaders (Individual Sections)

Each section component should implement skeleton loading:

```html
<!-- Example: dst-section.component.html -->

@if (loadingRisks() || loadingRecommendations()) {
  <div class="skeleton-loader p-4">
    <div class="flex items-center gap-3 mb-4">
      <div class="skeleton-box w-8 h-8 rounded-full"></div>
      <div class="skeleton-box w-48 h-6 rounded"></div>
    </div>
    <div class="skeleton-box w-full h-4 rounded mb-2"></div>
    <div class="skeleton-box w-3/4 h-4 rounded mb-2"></div>
    <div class="skeleton-box w-5/6 h-4 rounded"></div>
  </div>
} @else {
  <!-- Actual content -->
}
```

### 5. Completion Notification (Optional)

```typescript
// Auto-hide progress bar 2 seconds after completion
private _checkLoadingCompletion(): void {
  effect(() => {
    const progress = this.loadingProgress();
    if (progress.completed === progress.total) {
      // Show subtle completion toast
      setTimeout(() => {
        this.feedbackDialogService.showSuccessToast({
          summary: this.translateService.instant('message.loadingComplete'),
          detail: this.translateService.instant('message.allDataLoaded'),
          life: 2000
        });
      }, 500);
    }
  });
}
```

---

## Benefits of Recommended Approach

### User Experience
1. ✅ **Clear progress indication** - User knows what's loading and how much is left
2. ✅ **Non-intrusive** - Progress bar doesn't block content
3. ✅ **Predictable order** - Sections load top to bottom as displayed
4. ✅ **Fast perceived performance** - Top sections load first (above the fold)
5. ✅ **Professional appearance** - Skeleton loaders prevent layout shift

### Technical Benefits
1. ✅ **Maintains staggered loading** - Still prevents connection exhaustion
2. ✅ **No breaking changes** - Works with existing section components
3. ✅ **Easy to extend** - Add new sections to progress tracking
4. ✅ **Performance monitoring** - Track loading times per section
5. ✅ **Error handling** - Can show which sections failed to load

### Business Impact
1. ✅ **Reduced perceived wait time** - Users feel system is responsive
2. ✅ **Lower abandonment** - Users less likely to leave during loading
3. ✅ **Better engagement** - Users see content as it arrives
4. ✅ **Professional image** - Modern loading UX reflects well on UNOPS

---

## Implementation Phases

### Phase 1: Core Progress Tracking (Week 1)
- [ ] Add `LoadingProgress` interface and signal
- [ ] Implement `updateLoadingProgress()` method
- [ ] Add progress tracking to `_loadRecordDetails()`
- [ ] Create progress bar UI component
- [ ] Test with existing functionality

### Phase 2: Orchestrated Loading (Week 2)
- [ ] Implement `_orchestrateSectionLoading()` method
- [ ] Add section loading coordination
- [ ] Update child components to emit loading events
- [ ] Add progress updates for each section
- [ ] Test loading sequence order

### Phase 3: Section Skeletons (Week 3)
- [ ] Create reusable skeleton loader components
- [ ] Add skeletons to DST section
- [ ] Add skeletons to other data-loading sections
- [ ] Implement shimmer animation
- [ ] Test across different viewport sizes

### Phase 4: Polish & Testing (Week 4)
- [ ] Add completion notification
- [ ] Implement auto-hide logic
- [ ] Add error state handling
- [ ] Performance testing
- [ ] User acceptance testing
- [ ] Accessibility review

---

## Alternative Approaches Considered

### ❌ Modal Loading Overlay
**Why Not:** Too intrusive, blocks user from viewing loaded content

### ❌ Full-Page Spinner
**Why Not:** Doesn't show progress, feels slow even when fast

### ❌ Sequential Section Reveal
**Why Not:** Forces user to wait for all sections, slow overall experience

### ❌ No Loading Indication
**Why Not:** Current state, confusing for users

---

## Accessibility Considerations

1. **ARIA Live Regions** - Announce loading progress to screen readers
```html
<div 
  role="status" 
  aria-live="polite" 
  aria-atomic="true"
  class="sr-only"
>
  {{ progressMessage() }}
</div>
```

2. **Keyboard Navigation** - Don't trap focus during loading

3. **Reduced Motion** - Respect `prefers-reduced-motion` for animations
```scss
@media (prefers-reduced-motion: reduce) {
  .loading-progress-strip * {
    animation: none !important;
    transition: none !important;
  }
}
```

4. **Color Contrast** - Ensure progress indicators meet WCAG AA standards

---

## Translation Keys Required

Add to all 4 language files (en.json, es.json, fr.json, pt.json):

```json
{
  "message": {
    "loadingProgress": "Loading: {{section}} ({{current}} of {{total}} sections)",
    "allDataLoaded": "All data loaded successfully",
    "loadingComplete": "Loading Complete",
    "loadingSection": "Loading {{section}}...",
    "sectionLoadFailed": "Failed to load {{section}}"
  },
  "section": {
    "opportunityData": "Opportunity Data",
    "aiInsights": "AI Insights",
    "analysis": "Analysis",
    "riskAssessment": "Risk Assessment",
    "recommendations": "Recommendations",
    "similarOpportunities": "Similar Opportunities",
    "relatedItems": "Related Items",
    "documents": "Documents"
  }
}
```

---

## Performance Metrics to Track

1. **Time to First Meaningful Paint** - When does user see first section content?
2. **Time to Interactive** - When can user interact with page?
3. **Total Loading Time** - When does all data finish loading?
4. **Section Load Times** - How long does each section take?
5. **Failed Load Rate** - How often do sections fail to load?

### Target Metrics
- First Meaningful Paint: < 1 second
- Time to Interactive: < 2 seconds
- Total Loading Time: < 5 seconds
- Section Load Success Rate: > 99%

---

## Conclusion

The recommended hybrid approach provides:
- **Clear visibility** into loading progress
- **Non-intrusive** user experience
- **Professional** modern UI patterns
- **Maintainable** architecture for future expansion

This solution balances technical requirements (staggered loading to prevent connection exhaustion) with user experience (clear indication of what's happening).

**Next Steps:**
1. Review and approve recommendations
2. Create tickets for implementation phases
3. Begin Phase 1 development
4. Schedule user testing after Phase 2

