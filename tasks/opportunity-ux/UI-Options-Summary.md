# Opportunity UX Options - Summary & Recommendations

## Overview

This document provides a comprehensive comparison of three distinct UI/UX approaches for the Opportunity feature in UNOPS Opportunity+. Each option has been designed to address the complex data model and diverse user personas while integrating AI assistance throughout the workflow.

## Quick Reference Table

| Criteria | Option 1: Unified Dashboard | Option 2: Tabbed Organization | Option 3: Wizard Workflow |
|----------|----------------------------|------------------------------|---------------------------|
| **Primary Philosophy** | Everything visible at once | Focused sections, reduced clutter | Guided step-by-step progression |
| **Information Architecture** | Single scrolling page | Multiple tabs by category | Sequential steps |
| **Best User Type** | Experienced users | All user levels | New/infrequent users |
| **Cognitive Load** | High (all info visible) | Medium (one section at time) | Low (guided chunks) |
| **Initial Learning Curve** | Steep | Moderate | Gentle |
| **Creation Time (Experienced)** | ~25 min | ~30 min | ~35 min |
| **Creation Time (New User)** | ~45 min | ~35 min | ~25 min |
| **Mobile Compatibility** | Poor | Good | Excellent |
| **Collaboration** | Excellent | Good | Limited |
| **AI Visibility** | Always prominent | Tab-specific panel | Step-specific guidance |
| **Flexibility** | High | Medium | Low |
| **Completeness Enforcement** | Weak | Medium | Strong |
| **Update/Edit Efficiency** | Excellent | Good | Poor |
| **Review Efficiency** | Excellent | Good | Medium |

## Detailed Comparison

### 1. User Experience by Persona

#### Opportunity Manager (Primary Creator)

**Option 1 - Unified Dashboard**
- **Pros**: Can see all relationships, quick edits, no navigation overhead
- **Cons**: Overwhelming at first, requires scrolling, easy to miss fields
- **Best When**: Experienced with system, creating complex opportunities, needs to see big picture

**Option 2 - Tabbed Organization**
- **Pros**: Focused work environment, clear sections, manageable chunks
- **Cons**: Context switching between tabs, can't compare sections easily
- **Best When**: Moderately experienced, working on opportunities over multiple sessions

**Option 3 - Wizard Workflow**
- **Pros**: Clear guidance, hard to miss requirements, AI highly integrated
- **Cons**: Feels slow for experts, rigid workflow, more clicks required
- **Best When**: New to system, wants to ensure nothing is missed, values guidance

#### Team Member (Contributor/Editor)

**Option 1 - Unified Dashboard**
- **Pros**: Quick edits, can use Ctrl+F to find section, see related context
- **Cons**: Must scroll to find specific section, information density
- **Time to Update**: 5-7 minutes

**Option 2 - Tabbed Organization**
- **Pros**: Know which tab to go to, focused editing, less distraction
- **Cons**: Must switch tabs to see related information
- **Time to Update**: 7-10 minutes

**Option 3 - Wizard Workflow**
- **Pros**: Clear location for each type of edit, guided updates
- **Cons**: Must navigate through wizard or find quick-edit mode, overhead
- **Time to Update**: 8-12 minutes

#### Opportunity Authority (Reviewer/Decision Maker)

**Option 1 - Unified Dashboard**
- **Pros**: Complete picture immediately, no navigation, can scan quickly
- **Cons**: Information overload, must scroll for details
- **Time to Review**: 15-20 minutes
- **Decision Quality**: High (complete context)

**Option 2 - Tabbed Organization**
- **Pros**: Systematic review possible, DST insights concentrated, focused assessment
- **Cons**: Must remember details across tabs, can't compare easily
- **Time to Review**: 20-25 minutes
- **Decision Quality**: High (thorough review)

**Option 3 - Wizard Workflow**
- **Pros**: Step-by-step review matches creation flow, clear structure
- **Cons**: Must click through all steps, tedious navigation, no overview
- **Time to Review**: 22-28 minutes (wizard) OR 18-22 minutes (PDF export)
- **Decision Quality**: Medium-High (depends on review method)

#### AI System (Support Agent)

**Option 1 - Unified Dashboard**
- **Integration**: Persistent panel at top, real-time updates across all sections
- **Visibility**: Always visible, but must compete with dense information
- **Effectiveness**: High - can show cross-cutting insights

**Option 2 - Tabbed Organization**
- **Integration**: Collapsible panel adapts to each tab
- **Visibility**: Always present but contextual to current tab
- **Effectiveness**: High - relevant suggestions per section

**Option 3 - Wizard Workflow**
- **Integration**: Step-specific guidance panel, document extraction at start
- **Visibility**: Most prominent, integral to each step
- **Effectiveness**: Very High - natural integration with workflow

### 2. Feature Implementation Analysis

#### Core Features by Option

**Document Upload & AI Extraction**
- **Option 1**: Drop zone in Documents section, AI updates all sections dynamically
- **Option 2**: Documents tab with extraction, badges notify other tabs of changes
- **Option 3**: Step 1 dedicated to upload, extraction drives subsequent steps
- **Best Implementation**: Option 3 (most natural integration)

**Stakeholder Management**
- **Option 1**: Who section with inline cards, see all at once
- **Option 2**: Stakeholders tab with master-detail view
- **Option 3**: Step 3 with guided entry for each type
- **Best Implementation**: Option 2 (best balance of detail and organization)

**Timeline & Milestones**
- **Option 1**: When section with inline Gantt chart
- **Option 2**: Timeline tab with interactive calendar
- **Option 3**: Step 5 with date pickers and AI-generated milestones
- **Best Implementation**: Option 1 (visual context with other sections)

**Geographic Implementation**
- **Option 1**: Where section with map and country list side-by-side
- **Option 2**: Geography tab with full-width map and details
- **Option 3**: Step 5 country-by-country entry with context warnings
- **Best Implementation**: Option 2 (dedicated space for geographic visualization)

**DST Insights & Analysis**
- **Option 1**: Dedicated section toward bottom with full insights
- **Option 2**: Entire AI Insights tab with comprehensive DST report
- **Option 3**: Scattered throughout steps + summary in Step 6
- **Best Implementation**: Option 2 (consolidated analytical view)

**Collaboration & Comments**
- **Option 1**: Comment stream at bottom of page, always visible
- **Option 2**: Activity tab with full collaboration history
- **Option 3**: Notes section in each step + separate activity area
- **Best Implementation**: Option 1 (continuous thread, always accessible)

### 3. Technical Considerations

#### Performance

**Option 1 - Unified Dashboard**
- **Initial Load**: Heavy (all data at once) - 3-5 seconds
- **Subsequent Interactions**: Fast (everything in memory)
- **Optimization Needs**: Lazy loading, virtual scrolling, intersection observers
- **Scalability**: Challenging for very complex opportunities

**Option 2 - Tabbed Organization**
- **Initial Load**: Light (only Overview tab) - 1-2 seconds
- **Subsequent Interactions**: Medium (tab switching loads) - 0.5-1 second per tab
- **Optimization Needs**: Tab content caching, preloading next likely tab
- **Scalability**: Good for any opportunity size

**Option 3 - Wizard Workflow**
- **Initial Load**: Lightest (only Step 1) - <1 second
- **Subsequent Interactions**: Fast (small chunks) - 0.3-0.5 seconds per step
- **Optimization Needs**: State management, draft auto-save
- **Scalability**: Excellent for any opportunity size

#### Development Complexity

**Option 1 - Unified Dashboard**
- **Frontend Complexity**: Medium-High (manage many components on one page)
- **State Management**: Complex (everything updates in real-time)
- **Responsive Design**: Difficult (must adapt dense layout)
- **Estimated Development**: 8-10 weeks

**Option 2 - Tabbed Organization**
- **Frontend Complexity**: Medium (tab routing and content switching)
- **State Management**: Medium (per-tab state with cross-tab awareness)
- **Responsive Design**: Moderate (tabs adapt to dropdown on mobile)
- **Estimated Development**: 6-8 weeks

**Option 3 - Wizard Workflow**
- **Frontend Complexity**: Medium (step flow and validation)
- **State Management**: High (maintain state across steps, handle back navigation)
- **Responsive Design**: Easy (linear flow works well on any screen)
- **Estimated Development**: 7-9 weeks

#### Maintenance & Evolution

**Option 1 - Unified Dashboard**
- **Adding New Sections**: Challenging (must fit into scrolling layout)
- **Modifying Existing**: Medium (changes visible immediately, testing complex)
- **Version Updates**: Complex (any change affects entire page)

**Option 2 - Tabbed Organization**
- **Adding New Sections**: Easy (just add new tab)
- **Modifying Existing**: Easy (tabs are isolated)
- **Version Updates**: Moderate (tab-by-tab updates possible)

**Option 3 - Wizard Workflow**
- **Adding New Sections**: Medium (must fit into step sequence)
- **Modifying Existing**: Moderate (step changes affect flow)
- **Version Updates**: Complex (flow changes require careful testing)

### 4. Use Case Scenarios

#### Scenario 1: Quick Opportunity Creation (Simple Project)
- **Complexity**: Low (straightforward water project, single country, one partner)
- **Required Fields**: ~15 fields, 2 deliverables, 1 partner
- **Best Option**: Option 1 (fast entry, minimal navigation)
- **Time**: 10-12 minutes (Option 1) vs 15-18 minutes (Option 3)

#### Scenario 2: Complex Multi-Country Initiative
- **Complexity**: High (3 countries, 2 funding partners, 5 deliverables, 8 stakeholders)
- **Required Fields**: ~40 fields, extensive relationships
- **Best Option**: Option 2 (organized by section, can focus per area)
- **Time**: 30-35 minutes (Option 2) vs 25-30 minutes (Option 3 with AI help)

#### Scenario 3: Team Collaboration on Draft
- **Complexity**: Medium (work in progress, multiple people contributing)
- **Use Case**: Different team members working on different aspects
- **Best Option**: Option 1 or 2 (can work simultaneously on different sections)
- **Collaboration**: Option 1 > Option 2 >> Option 3

#### Scenario 4: Authority Review & Decision
- **Complexity**: Medium-High (thorough review required for Go/No-Go)
- **Use Case**: Decision maker needs complete context
- **Best Option**: Option 1 (complete view) or Option 2 (systematic review)
- **Decision Time**: Option 1 (15-20 min) < Option 2 (20-25 min) < Option 3 (25-30 min)

#### Scenario 5: Mobile Opportunity Creation
- **Complexity**: Any level
- **Use Case**: Field staff creating opportunity on tablet
- **Best Option**: Option 3 (wizard designed for mobile)
- **Mobile UX**: Option 3 >>> Option 2 > Option 1

#### Scenario 6: Updating Existing Opportunity
- **Complexity**: Low (change one or two fields)
- **Use Case**: Quick edit after initial creation
- **Best Option**: Option 1 (direct access) or Option 2 (jump to tab)
- **Update Time**: Option 1 (5 min) < Option 2 (7 min) << Option 3 (12 min)

### 5. Recommendation Framework

#### Choose Option 1 (Unified Dashboard) When:
✅ Users are experienced and familiar with the system
✅ Desktop/large screen is primary access method
✅ Opportunities are moderately complex (not too simple, not extremely large)
✅ Quick reviews and decisions are frequent
✅ Team collaboration is important
✅ Users value seeing relationships between sections
✅ Performance is acceptable (good network, modern devices)

**Recommended For**: Experienced Opportunity Managers, internal UNOPS staff, desktop-first environment

#### Choose Option 2 (Tabbed Organization) When:
✅ User skill levels vary (new to experienced)
✅ Opportunities range from simple to complex
✅ Both desktop and mobile access are important
✅ Focused work without distraction is valued
✅ System will scale to many features over time
✅ DST insights should be prominent and consolidated
✅ Performance and scalability are priorities

**Recommended For**: Mixed user base, scalable long-term solution, balanced approach

#### Choose Option 3 (Wizard Workflow) When:
✅ Users are new or infrequent (need guidance)
✅ Mobile/tablet access is critical
✅ Completeness and quality are more important than speed
✅ AI assistance is core differentiator
✅ Training resources are limited
✅ Opportunities follow clear sequential workflow
✅ Initial creation is more important than updates

**Recommended For**: New users, mobile-first, AI-driven guided experience

### 6. Hybrid Approach Recommendation

Based on the analysis, a **hybrid approach** may provide the best overall experience:

#### Recommended Hybrid Model

**For Opportunity Creation**: Option 3 (Wizard Workflow)
- First-time creation uses guided wizard
- AI document extraction at start
- Step-by-step validation ensures completeness
- Lower training burden for new users

**For Opportunity Editing**: Option 2 (Tabbed Organization)
- After initial creation, switch to tabbed interface
- Allows focused editing without wizard overhead
- Better for team collaboration on in-progress opportunities
- More efficient for updates

**For Opportunity Review**: Option 1 (Unified Dashboard) OR PDF Export
- Authorities get consolidated view for decision-making
- Can print/export complete opportunity
- All context visible for assessment
- Optional: Click to expand detailed sections

#### Implementation Strategy

1. **Phase 1**: Implement Option 3 (Wizard) for creation
   - Focus on new opportunity flow
   - Perfect AI integration
   - Get users comfortable with system

2. **Phase 2**: Add Option 2 (Tabs) for editing
   - Switch to tabs after first save/submit
   - Enable efficient updates and collaboration
   - Maintain wizard option for "Start Over"

3. **Phase 3**: Add Option 1 (Dashboard) view mode
   - Read-only consolidated view
   - Use for reviews and printing
   - Optional: Enable inline editing for power users

### 7. Final Recommendation

**Primary Recommendation**: **Option 2 - Tabbed Organization** with elements from other options

**Rationale**:
1. **Balanced Approach**: Works for all user skill levels
2. **Scalable**: Easy to add features and maintain over time
3. **Performance**: Good initial load time, scales to large opportunities
4. **Mobile-Friendly**: Adaptable to smaller screens
5. **AI Integration**: Can showcase AI prominently without overwhelming
6. **Flexibility**: Supports both creation and editing well
7. **Collaboration**: Multiple users can work on different tabs

**Enhancements from Other Options**:
- Add **Quick Start Wizard** (from Option 3) for first-time users with option to skip
- Add **AI Document Upload** at start (from Option 3) to jump-start creation
- Add **Unified Export View** (from Option 1) for printing and review
- Add **Progress Dashboard** (from Option 3) showing completion status
- Add **Persistent AI Panel** (from Option 1) that adapts per tab

**Alternative Recommendation**: If mobile access is critical and users are primarily new/infrequent, choose **Option 3 (Wizard)** with ability to switch to **Option 2 (Tabs)** after first submission.

---

## User Testing Recommendations

Before final decision, conduct user testing with:

1. **5-7 Opportunity Managers** (varying experience levels)
   - Test all three prototypes for opportunity creation
   - Measure completion time, error rate, satisfaction
   - Observe where users struggle or excel

2. **3-5 Team Members** (contributors)
   - Test editing workflows in each option
   - Measure update efficiency
   - Assess collaboration effectiveness

3. **3-4 Opportunity Authorities** (decision makers)
   - Test review and decision workflow
   - Measure decision time and confidence
   - Assess information accessibility

4. **Mobile/Tablet Testing**
   - Test each option on tablet (iPad, Android)
   - Assess usability on smaller screens
   - Identify mobile-specific issues

**Key Metrics to Track**:
- Time to complete opportunity creation
- Number of errors or missed required fields
- User satisfaction scores (1-10)
- Navigation clicks/actions required
- Perceived cognitive load (survey)
- Decision confidence (for authorities)

---

## Conclusion

Each UI option has distinct strengths:
- **Option 1**: Best for experienced users who need complete context
- **Option 2**: Best balanced approach for diverse user base and long-term scalability
- **Option 3**: Best for new users and mobile-first scenarios

The **recommended approach is Option 2 (Tabbed Organization)** enhanced with elements from the other options to create a flexible, scalable, and user-friendly experience that serves all personas effectively while maintaining the ability to evolve over time.

