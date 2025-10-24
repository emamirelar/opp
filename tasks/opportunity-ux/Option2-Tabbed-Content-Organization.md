# UI Option 2: Tabbed Content Organization

## Overview
A multi-tab interface that organizes opportunity information into logical sections, reducing cognitive overload while maintaining easy navigation between related content. This approach balances focus with accessibility.

## ASCII Layout Diagram

```
┌─────────────────────────────────────────────────────────────────────────────────────────────────┐
│ OPPORTUNITY+ │ Water Infrastructure Initiative [OPP-12345]                                      │
│ ◀ Back to List │ Partner Ref: ABC-2024-001                    Status: Draft  Stage: Profiling   │
├─────────────────────────────────────────────────────────────────────────────────────────────────┤
│                                                                                                   │
│  ┌────────────────────────────────────────────────────────────────────────────────────────────┐ │
│  │ 👤 Opportunity Manager: Sarah Chen         Org Unit: Regional Office - Asia Pacific        │ │
│  │ 💰 Budget: $2.5M USD    📅 Target Signing: Q3 2025    🌍 Countries: 3    ⚠️ Risks: 2       │ │
│  └────────────────────────────────────────────────────────────────────────────────────────────┘ │
│                                                                                                   │
│ ┌─────────────────────────────────────────────────────────────────────────────────────────────┐ │
│ │ [📋 Overview] [👥 Stakeholders] [💰 Finances] [📅 Timeline] [🌍 Geography] [📄 Documents]  │ │
│ │ [🎯 Impact]   [🤖 AI Insights]  [💬 Activity]  [⚙️ Settings]                               │ │
│ └─────────────────────────────────────────────────────────────────────────────────────────────┘ │
│                                                                                                   │
│  ┌────────────────────────────────────────────────────────────────────────────────────────────┐ │
│  │ 🤖 AI ASSISTANT - Active Assistance                          [Minimize] [🔄 Refresh]        │ │
│  ├────────────────────────────────────────────────────────────────────────────────────────────┤ │
│  │ ⚡ ACTIVE SUGGESTIONS (3)                                                                   │ │
│  │ • Target Signing Date is missing - this is required for Go/No-Go decision                  │ │
│  │   [Set Date] [Remind Later]                                                                │ │
│  │ • Based on deliverables, consider adding SDG 13 (Climate Action)                           │ │
│  │   [Add SDG] [Dismiss]                                                                      │ │
│  │ • Found 3 similar opportunities with relevant lessons learned                              │ │
│  │   [View Similar] [Dismiss]                                                                 │ │
│  │                                                                                            │ │
│  │ 📊 CURRENT ANALYSIS STATUS:                                                                │ │
│  │ ✅ Completeness: 78%  |  ✅ Complexity Score: 7.2/10  |  ⚠️ Risk Level: Medium-High        │ │
│  │ [View Full DST Report]                                                                     │ │
│  └────────────────────────────────────────────────────────────────────────────────────────────┘ │
│                                                                                                   │
│                                                                                                   │
│  ════════════════════════════════════════════════════════════════════════════════════════════   │
│  CURRENT TAB: OVERVIEW                                                                            │
│  ════════════════════════════════════════════════════════════════════════════════════════════   │
│                                                                                                   │
│  ┌────────────────────────────────────────────────────────────────────────────────────────────┐ │
│  │ 📝 BASIC INFORMATION                                                     [✏️ Edit Section] │ │
│  ├────────────────────────────────────────────────────────────────────────────────────────────┤ │
│  │                                                                                            │ │
│  │ Opportunity Name: [Water Infrastructure Initiative                              ]          │ │
│  │                                                                                            │ │
│  │ Description:                                                                               │ │
│  │ ┌────────────────────────────────────────────────────────────────────────────────────────┐│ │
│  │ │ Comprehensive water infrastructure development program targeting rural communities in  ││ │
│  │ │ South Asia. Focus on sustainable water access, local capacity building, and long-term  ││ │
│  │ │ maintenance systems. Includes construction of 50 water points, training of 200         ││ │
│  │ │ community technicians, and establishment of monitoring systems.                        ││ │
│  │ └────────────────────────────────────────────────────────────────────────────────────────┘│ │
│  │                                                                                            │ │
│  │ Partner Reference:        [ABC-2024-001                      ]                            │ │
│  │ Responsible Org Unit:     [Regional Office - Asia Pacific    ▼]                           │ │
│  │ Partnership Agreement:    [UNOPS-WorldBank-MOU-2023          ▼] [📄 View]                │ │
│  │ Proposed Initiative Type: [● Project  ○ Programme  ○ Portfolio]                          │ │
│  │ Initiative Budget (USD):  [$2,500,000.00                     ]                            │ │
│  │                                                                                            │ │
│  │ [💾 Save Changes]  [❌ Cancel]                                                             │ │
│  └────────────────────────────────────────────────────────────────────────────────────────────┘ │
│                                                                                                   │
│  ┌────────────────────────────────────────────────────────────────────────────────────────────┐ │
│  │ 📦 DELIVERABLES (3)                                           [+ Add Deliverable] [⚙️ Bulk]│ │
│  ├────────────────────────────────────────────────────────────────────────────────────────────┤ │
│  │                                                                                            │ │
│  │ ┌──────────────────────────────────────────────────────────────────────────────────────┐  │ │
│  │ │ 1️⃣ Infrastructure Development                                      [✏️ Edit] [🗑️ Delete]│  │ │
│  │ ├──────────────────────────────────────────────────────────────────────────────────────┤  │ │
│  │ │ Construction of 50 water points across implementation regions with sustainable       │  │ │
│  │ │ design and community ownership model.                                                │  │ │
│  │ │                                                                                      │  │ │
│  │ │ Service Line: Infrastructure / Water & Sanitation                                    │  │ │
│  │ │ 🤖 AI Note: Similar deliverable in "Water Supply Nepal 2023" - Budget: $900K        │  │ │
│  │ └──────────────────────────────────────────────────────────────────────────────────────┘  │ │
│  │                                                                                            │ │
│  │ ┌──────────────────────────────────────────────────────────────────────────────────────┐  │ │
│  │ │ 2️⃣ Capacity Building                                           [✏️ Edit] [🗑️ Delete]│  │ │
│  │ ├──────────────────────────────────────────────────────────────────────────────────────┤  │ │
│  │ │ Training of 200 community technicians in water system operation, maintenance, and   │  │ │
│  │ │ basic repairs. Includes certification program and ongoing support.                  │  │ │
│  │ │                                                                                      │  │ │
│  │ │ Service Line: Capacity Development / Technical Training                             │  │ │
│  │ └──────────────────────────────────────────────────────────────────────────────────────┘  │ │
│  │                                                                                            │ │
│  │ ┌──────────────────────────────────────────────────────────────────────────────────────┐  │ │
│  │ │ 3️⃣ Monitoring & Evaluation                                     [✏️ Edit] [🗑️ Delete]│  │ │
│  │ ├──────────────────────────────────────────────────────────────────────────────────────┤  │ │
│  │ │ Establishment of quarterly assessment system for water quality, system functionality,│  │ │
│  │ │ and community satisfaction tracking.                                                 │  │ │
│  │ │                                                                                      │  │ │
│  │ │ Service Line: Project Management / M&E                                               │  │ │
│  │ └──────────────────────────────────────────────────────────────────────────────────────┘  │ │
│  │                                                                                            │ │
│  └────────────────────────────────────────────────────────────────────────────────────────────┘ │
│                                                                                                   │
│  ┌────────────────────────────────────────────────────────────────────────────────────────────┐ │
│  │ 📊 COMPLETION STATUS                                                                       │ │
│  ├────────────────────────────────────────────────────────────────────────────────────────────┤ │
│  │                                                                                            │ │
│  │ Overall Progress: ████████████████████░░░░░░ 78%                                          │ │
│  │                                                                                            │ │
│  │ ✅ Basic Information:     Complete (100%)                                                 │ │
│  │ ✅ Deliverables:         Complete (100%)                                                  │ │
│  │ ⚠️  Stakeholders:        Incomplete (60%) - Missing client contact information            │ │
│  │ ⚠️  Finances:            Incomplete (75%) - Fee calculations need review                  │ │
│  │ ❌ Timeline:             Incomplete (40%) - Target Signing Date required                  │ │
│  │ ✅ Geography:            Complete (100%)                                                  │ │
│  │ ✅ Impact Alignment:     Complete (100%)                                                  │ │
│  │ ⚠️  Documents:           Incomplete (50%) - Risk assessment document pending              │ │
│  │                                                                                            │ │
│  │ [View Missing Requirements]                                                               │ │
│  └────────────────────────────────────────────────────────────────────────────────────────────┘ │
│                                                                                                   │
│  ┌────────────────────────────────────────────────────────────────────────────────────────────┐ │
│  │ 🔗 QUICK LINKS                                                                             │ │
│  ├────────────────────────────────────────────────────────────────────────────────────────────┤ │
│  │ → View All Funding Partners (2)     → View Implementation Countries (3)                   │ │
│  │ → View All Stakeholders (5)         → View All Documents (8)                              │ │
│  │ → Review DST Analysis Report        → Generate Draft Budget                               │ │
│  │ → View Similar Opportunities (3)    → Export Opportunity Statement                        │ │
│  └────────────────────────────────────────────────────────────────────────────────────────────┘ │
│                                                                                                   │
│                                                                                                   │
│  ┌────────────────────────────────────────────────────────────────────────────────────────────┐ │
│  │                                            ACTION BAR                                       │ │
│  │ [💾 Save] [📤 Submit for Review] [📋 Export PDF] [🔄 Refresh Data] [⚙️ Opportunity Settings]│ │
│  └────────────────────────────────────────────────────────────────────────────────────────────┘ │
└─────────────────────────────────────────────────────────────────────────────────────────────────┘
```

### Other Tab Views (Abbreviated)

#### STAKEHOLDERS TAB
```
┌──────────────────────────────────────────────────────────────────────────────────┐
│ [📋 Overview] [👥 Stakeholders] [💰 Finances] [📅 Timeline] [🌍 Geography] ...  │
└──────────────────────────────────────────────────────────────────────────────────┘

┌────────────────────────────────┬────────────────────────────────────────────────┐
│ 💰 FUNDING PARTNERS (2)        │ DETAILS: World Bank                            │
├────────────────────────────────┤                                                │
│ ┌────────────────────────────┐ │ Partner Type: Multilateral Development Bank    │
│ │ 🏦 World Bank              │ │ Funded Amount: $1,800,000.00 USD              │
│ │ $1.8M USD (72%)            │ │ Fee: 7% ($126,000)                            │
│ │ [View] [Edit]              │ │ Fee Type: Percentage-based                    │
│ └────────────────────────────┘ │                                                │
│ ┌────────────────────────────┐ │ Partnership Agreement: UNOPS-WB-MOU-2023      │
│ │ 🇪🇺 EU Commission          │ │ Commitment Status: Confirmed                  │
│ │ €700K EUR (28%)            │ │ Payment Terms: Quarterly disbursements        │
│ │ [View] [Edit]              │ │                                                │
│ └────────────────────────────┘ │ 🤖 AI Note: This partner has funded 12        │
│ [+ Add Funding Partner]        │ similar water infrastructure projects          │
│                                │ [View Partner History]                         │
├────────────────────────────────┼────────────────────────────────────────────────┤
│ 🏢 CLIENT PARTNERS (1)         │                                                │
│ ┌────────────────────────────┐ │                                                │
│ │ 🏛️ Ministry of Water       │ │                                                │
│ │ Resources                  │ │                                                │
│ │ [View] [Edit]              │ │                                                │
│ └────────────────────────────┘ │                                                │
│ [+ Add Client Partner]         │                                                │
├────────────────────────────────┤                                                │
│ 👥 INTERNAL STAKEHOLDERS (3)   │                                                │
│ • Sarah Chen (Opportunity Mgr) │                                                │
│ • James Wilson (Tech Lead)     │                                                │
│ • Maria Garcia (Finance)       │                                                │
│ [+ Add Stakeholder]            │                                                │
├────────────────────────────────┤                                                │
│ 🌐 EXTERNAL STAKEHOLDERS (2)   │                                                │
│ • Dr. Ahmed Hassan (Government)│                                                │
│ • Lisa Park (NGO Director)     │                                                │
│ [+ Add Stakeholder]            │                                                │
└────────────────────────────────┴────────────────────────────────────────────────┘
```

#### AI INSIGHTS TAB
```
┌──────────────────────────────────────────────────────────────────────────────────┐
│ [📋 Overview] ... [🤖 AI Insights] [💬 Activity]  [⚙️ Settings]                 │
└──────────────────────────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────────────────────────┐
│ 🎲 DECISION SUPPORT TOOL (DST) ANALYSIS                Last Updated: 2 hours ago│
├─────────────────────────────────────────────────────────────────────────────────┤
│                                                                                 │
│ OVERALL COMPLEXITY SCORE: 7.2/10 (Medium-High)        [🔄 Refresh Analysis]    │
│ ████████████████████████████████████████████░░░░░░░░░░                        │
│                                                                                 │
│ [Strategic Alignment] [Partners] [Implementation] [Context] [Scope] [Timeline] │
│ [Budget] [Impact] [Safeguards]                                                 │
│                                                                                 │
│ ┌─────────────────────────────────────────────────────────────────────────────┐│
│ │ STRATEGIC ALIGNMENT - Score: 8.5/10  ✅                                     ││
│ ├─────────────────────────────────────────────────────────────────────────────┤│
│ │ This opportunity strongly aligns with UNOPS Strategic Plan 2022-2025,       ││
│ │ specifically under the Infrastructure practice area and Regional Strategy   ││
│ │ for Water Security. The focus on sustainable water access addresses critical││
│ │ needs in fragile contexts.                                                  ││
│ │                                                                             ││
│ │ ✅ Strengths:                                                               ││
│ │ • Direct alignment with SDG 6 (Clean Water)                                ││
│ │ • Supports regional water security objectives                              ││
│ │ • Multi-country approach increases impact scale                            ││
│ │                                                                             ││
│ │ ⚠️  Considerations:                                                         ││
│ │ • Verify alignment with country-level UN Cooperation Frameworks            ││
│ │ • Consider gender mainstreaming requirements                               ││
│ │                                                                             ││
│ │ 🔍 Similar Opportunities: 3 found with similar strategic focus             ││
│ │ [View Similar Opportunities]                                               ││
│ └─────────────────────────────────────────────────────────────────────────────┘│
│                                                                                 │
│ ┌─────────────────────────────────────────────────────────────────────────────┐│
│ │ ⚠️  RISKS IDENTIFIED (4)                           [Add All to Risk Register]││
│ ├─────────────────────────────────────────────────────────────────────────────┤│
│ │ 🔴 HIGH: Political instability in Myanmar implementation areas              ││
│ │    Source: Country risk profile, Recent conflict analysis                  ││
│ │    Recommendation: Develop contingency plan and early warning system       ││
│ │    [Add to Register] [View Details] [Dismiss]                             ││
│ │                                                                             ││
│ │ 🟡 MEDIUM: Limited local technical capacity for system maintenance         ││
│ │    Source: Similar project lessons learned (Nepal 2023)                    ││
│ │    Recommendation: Extend capacity building timeline by 3 months           ││
│ │    [Add to Register] [View Details] [Dismiss]                             ││
│ │                                                                             ││
│ │ 🟡 MEDIUM: Monsoon season constraints on construction activities           ││
│ │    Source: Weather pattern analysis for Bangladesh, Nepal                  ││
│ │    Recommendation: Adjust timeline to avoid peak monsoon (June-Sept)       ││
│ │    [Add to Register] [View Details] [Dismiss]                             ││
│ │                                                                             ││
│ │ 🟡 MEDIUM: EUR currency fluctuation risk (€700K commitment)                ││
│ │    Source: Financial risk assessment, Historical volatility data           ││
│ │    Recommendation: Include currency hedging clause in partner agreement    ││
│ │    [Add to Register] [View Details] [Dismiss]                             ││
│ └─────────────────────────────────────────────────────────────────────────────┘│
│                                                                                 │
│ ┌─────────────────────────────────────────────────────────────────────────────┐│
│ │ 💡 RECOMMENDATIONS (5)                                    [Accept All]       ││
│ ├─────────────────────────────────────────────────────────────────────────────┤│
│ │ 1. Add gender advisor to development team                                   ││
│ │    Rationale: Required for water infrastructure projects in these contexts ││
│ │    [✅ Accept] [❌ Dismiss] [💬 Comment]                                     ││
│ │                                                                             ││
│ │ 2. Consider phased implementation approach                                  ││
│ │    Rationale: Reduce risk by starting with Bangladesh, then Nepal, Myanmar ││
│ │    [✅ Accept] [❌ Dismiss] [💬 Comment]                                     ││
│ │                                                                             ││
│ │ 3. Conduct early environmental and social impact assessment                ││
│ │    Rationale: Required for infrastructure projects of this scale           ││
│ │    [✅ Accept] [❌ Dismiss] [💬 Comment]                                     ││
│ └─────────────────────────────────────────────────────────────────────────────┘│
│                                                                                 │
│ ┌─────────────────────────────────────────────────────────────────────────────┐│
│ │ 🔍 SIMILAR OPPORTUNITIES (3)                       [Compare All] [View More]││
│ ├─────────────────────────────────────────────────────────────────────────────┤│
│ │ 1. Water Infrastructure Development - Nepal 2023        Relevance: 89%  ★★★★││
│ │    Budget: $2.1M | Duration: 18 months | Status: Completed Successfully    ││
│ │    Key Lessons: Early community engagement critical; extend training period││
│ │    [View Full Details] [Copy Structure] [View Lessons Learned]             ││
│ │                                                                             ││
│ │ 2. Rural Water Supply Program - Bangladesh 2022        Relevance: 85%  ★★★★││
│ │    Budget: $3.2M | Duration: 24 months | Status: Completed Successfully    ││
│ │    Key Lessons: Monsoon delays significant; budget 20% contingency         ││
│ │    [View Full Details] [Copy Structure] [View Lessons Learned]             ││
│ └─────────────────────────────────────────────────────────────────────────────┘│
└─────────────────────────────────────────────────────────────────────────────────┘
```

## Layout & UX Notes

### Navigation Philosophy
- **Tab-Based Organization**: Information grouped by logical categories reducing cognitive load
- **Persistent Context Bar**: Key metrics always visible across all tabs
- **AI Assistant Panel**: Collapsible but always present to provide real-time guidance
- **Master-Detail Pattern**: List view on left, detailed information on right (in some tabs)

### Key UX Features

1. **Focused Work Environment**
   - Only one category of information visible at a time
   - Reduces distractions and helps users concentrate on specific tasks
   - Clear tab labels with icons for easy identification

2. **Smart Tab Badges**
   - Completion percentage indicators on each tab
   - Warning badges for tabs with missing required information
   - Notification dots for tabs with AI suggestions or updates

3. **Contextual AI Integration**
   - AI assistant panel adapts content based on current tab
   - Suggestions relevant to the information being viewed
   - One-click actions to accept AI recommendations

4. **Progress Tracking**
   - Completion status section on Overview tab
   - Visual progress bars showing readiness for Go/No-Go decision
   - Clear indicators of what's required vs. optional

5. **Quick Access**
   - "Quick Links" section provides shortcuts to commonly accessed areas
   - Breadcrumb navigation shows current location
   - Recent tabs remembered for easy return

## Pros

✅ **Reduced Cognitive Load**
- Only relevant information visible at any time
- Less scrolling required within each tab
- Cleaner, more focused interface

✅ **Task-Oriented Workflow**
- Users can focus on one aspect at a time (e.g., just finances)
- Natural workflow progression through tabs
- Team members can work on different tabs simultaneously

✅ **Scalable Organization**
- Easy to add new tabs for future features
- Can accommodate simple or complex opportunities
- Information architecture remains clear even with large datasets

✅ **Better Performance**
- Lazy loading of tab content improves initial load time
- Only active tab data needs to be in memory
- Reduces server load and network traffic

✅ **Mobile/Tablet Friendly**
- Tabs collapse to dropdown menu on smaller screens
- Each tab can be optimized for mobile layout independently
- Less horizontal scrolling required

✅ **Clear Progress Indicators**
- Completion status immediately visible
- Easy to identify what's missing
- Tab badges show warnings without opening tab

## Cons

❌ **Context Switching**
- Need to switch tabs to see related information
- Can't compare sections side-by-side easily
- May forget information seen in other tabs

❌ **Navigation Overhead**
- Extra clicks required to access different information
- Can be frustrating for experienced users who know what they need
- Tab switching interrupts workflow

❌ **Hidden Information**
- Important information may be in tabs users don't open
- Risk of missing AI suggestions in inactive tabs
- No single-view for printing or exporting

❌ **AI Integration Challenges**
- AI panel must change content with each tab
- Cross-tab recommendations harder to surface
- May need to check multiple tabs to see all AI suggestions

❌ **Collaboration Friction**
- Harder to discuss opportunity when everyone looking at different tabs
- Comments section separated from content (in Activity tab)
- Screen sharing requires tab switching to show different aspects

❌ **Learning Curve**
- New users must learn where information is located
- Tab organization may not match mental model of all users
- Requires understanding of information architecture

## Persona Task Workflows

### Opportunity Manager: Creating a New Opportunity

1. **Initial Setup**
   - Clicks "Create New Opportunity" from list view
   - Lands on **Overview tab** with empty basic information form
   - AI panel shows: "Get started by uploading a concept note or entering details manually"

2. **Basic Information Entry**
   - Fills in: Name, Description, Org Unit, Budget estimate
   - Clicks "Save Changes" - green checkmark appears
   - Progress indicator updates: Overview 60% complete

3. **AI-Assisted Document Upload**
   - Clicks **Documents tab**
   - Drags concept note into upload zone
   - AI processes document (loading indicator for 10 seconds)
   - AI panel shows: "Extracted information ready for review" with badge notification on other tabs

4. **Review Extracted Data**
   - Clicks **Stakeholders tab** (shows badge "3 new suggestions")
   - Reviews AI-extracted funding partners: World Bank, EU Commission
   - Accepts both with one click "Accept All Suggestions"
   - Manually adds internal stakeholders from team

5. **Complete Financial Information**
   - Clicks **Finances tab**
   - Reviews pre-filled partner amounts from AI extraction
   - Adjusts fee percentages: World Bank 7%, EU 5%
   - Tab badge changes from ⚠️ to ✅

6. **Geographic Setup**
   - Clicks **Geography tab**
   - Adds countries: Bangladesh, Nepal, Myanmar
   - AI immediately flags Myanmar in AI panel: "High risk context detected"
   - Enters specific implementation areas for each country

7. **Timeline & Impact**
   - Clicks **Timeline tab**, enters target dates
   - AI generates suggested milestones based on similar opportunities
   - Accepts milestone timeline
   - Clicks **Impact tab**, adds SDG alignments (AI suggested SDG 6, user accepts)

8. **Review & Submit**
   - Returns to **Overview tab** to check completion status
   - All sections now show 85%+ complete
   - Clicks **AI Insights tab** to review DST analysis
   - Notes complexity score of 7.2/10
   - Clicks **Activity tab**, adds comment: "@James @Maria please review before submission"
   - Clicks "Submit for Review" button on action bar

**Time to Complete**: 30-35 minutes (with AI assistance)
**Tab Switches**: ~12 tab switches to complete all sections
**Cognitive Load**: Low-Medium - focused attention on one section at a time

---

### Team Member: Updating Deliverables

1. **Navigate to Opportunity**
   - Opens opportunity from notification: "Sarah Chen mentioned you in Water Infrastructure Initiative"
   - Lands on **Overview tab** by default

2. **Navigate to Deliverables**
   - Deliverables section visible on Overview tab
   - Clicks "Edit" on Deliverable #2
   - Inline editor opens with current text

3. **Update Deliverable**
   - Modifies description text for capacity building deliverable
   - AI panel suggests: "Consider adding service line classification"
   - Accepts AI suggestion
   - Clicks "Save Changes"

4. **Add New Deliverable**
   - Clicks "+ Add Deliverable"
   - Modal opens for new deliverable entry
   - Fills: Name, Description
   - AI auto-suggests service line based on keywords
   - Accepts and saves

5. **Respond to Comment**
   - Badge notification appears on **Activity tab** (number "1")
   - Clicks **Activity tab**
   - Sees Sarah's comment
   - Replies: "Deliverables updated. Added environmental monitoring as 4th deliverable per partner request."
   - Sarah receives instant notification

**Time to Complete**: 7-10 minutes
**Tab Switches**: 2 (Overview → Activity → Done)
**Cognitive Load**: Low - straightforward task with focused interface

---

### System / AI: Providing Intelligent Assistance

1. **Continuous Monitoring**
   - Monitors user activity across all tabs
   - Detects user is on **Overview tab** entering basic information
   - AI panel shows: "Great start! 3 required fields still needed"

2. **Tab-Specific Assistance**
   - User switches to **Geography tab**
   - AI panel immediately updates to show geography-related assistance
   - User adds "Myanmar" to country list
   - AI triggers risk analysis for Myanmar

3. **Context Retrieval**
   - Queries country risk database for Myanmar
   - Retrieves recent conflict data and fragile state classification
   - Updates AI panel: "⚠️ Myanmar flagged as high-risk context"
   - Adds badge notification to **AI Insights tab**: "New risk identified"

4. **Cross-Tab Intelligence**
   - User on **Finances tab** reviewing budget
   - AI panel shows: "Note: Myanmar implementation may require 15% contingency (see risks in AI Insights tab)"
   - Provides quick link to relevant section

5. **Proactive Analysis**
   - User completes 75% of required fields
   - AI automatically triggers full DST analysis
   - Generates comprehensive report in **AI Insights tab**
   - Shows notification badge and updates AI panel: "DST analysis complete - complexity score 7.2/10"

6. **Similar Opportunity Detection**
   - Based on deliverables, budget, and countries, AI searches knowledge base
   - Identifies 3 similar past opportunities
   - Extracts lessons learned from each
   - Surfaces in **AI Insights tab** with relevance scores

7. **Adaptive Recommendations**
   - Based on Myanmar risk + infrastructure scope + budget level
   - Generates recommendation: "Consider phased approach"
   - Shows in AI panel on **Overview tab** and details in **AI Insights tab**
   - User can accept/dismiss with one click

**AI Processing Strategy**:
- Light analysis: Real-time as user types/selects
- Medium analysis: Triggered on tab switch or major field completion
- Heavy analysis: Batch processing when opportunity reaches 70%+ completion
- Smart caching: Store results to avoid re-analysis on every tab switch

---

### Opportunity Authority: Making Go/No-Go Decision

1. **Review Notification**
   - Receives email: "Opportunity OPP-12345 submitted for your review"
   - Clicks link, opens opportunity in browser

2. **Initial Assessment**
   - Lands on **Overview tab**
   - Context bar shows key metrics: $2.5M budget, 3 countries, Draft stage
   - Scans completion status: 85% complete overall
   - AI panel summary: "Medium-high complexity, 4 risks identified, 3 recommendations"

3. **Quick Overview Review**
   - Reads basic information and description
   - Reviews 3 deliverables in collapsed cards
   - Notes quick links section at bottom
   - Decides to do deeper dive

4. **Financial Review**
   - Clicks **Finances tab**
   - Reviews funding partners: $1.8M (World Bank) + €700K (EU)
   - Checks fee calculations: 7% and 5%
   - Notes currency risk (EUR) - mental note to check risks

5. **Stakeholder Verification**
   - Clicks **Stakeholders tab**
   - Verifies opportunity manager (Sarah Chen) is appropriate
   - Reviews technical lead and finance officer assignments
   - Notes external stakeholders include government and NGO

6. **Timeline Feasibility**
   - Clicks **Timeline tab**
   - Reviews target signing date: Q3 2025
   - Examines milestone timeline
   - Considers if timeline is realistic given complexity

7. **Risk Assessment**
   - Clicks **AI Insights tab** (where the critical analysis lives)
   - Reviews DST overall score: 7.2/10
   - Reads through each profile dimension
   - **Focuses on Risks section**:
     * High risk: Myanmar political instability
     * Medium: Limited local capacity
     * Medium: Monsoon season constraints
     * Medium: EUR currency risk
   - Clicks "View Details" on Myanmar risk to read full analysis

8. **Similar Opportunity Comparison**
   - Scrolls to similar opportunities section on AI Insights tab
   - Clicks "View Full Details" for Nepal 2023 water project
   - Reads lessons learned: "Early community engagement critical"
   - Notes success rate and budget alignment

9. **Review Recommendations**
   - Returns to recommendations section on AI Insights tab
   - Reads AI recommendation: "Add gender advisor"
   - Reads: "Consider phased implementation"
   - Reads: "Conduct early environmental assessment"
   - Agrees with all three recommendations

10. **Geographic Context**
    - Clicks **Geography tab** to review implementation areas
    - Views map showing 3 countries
    - Reads specific areas for each country
    - Confirms understanding of coverage

11. **Make Decision**
    - Clicks **Activity tab**
    - Adds comment: 
      ```
      **DECISION: Conditional GO** ✅
      
      Approved to proceed to development with the following conditions:
      1. Add gender advisor to team (per AI recommendation - accepted)
      2. Implement phased approach: Bangladesh → Nepal → Myanmar
      3. Complete environmental impact assessment before Week 5
      4. Develop detailed risk mitigation plan for Myanmar context
      5. Include currency hedging clause for EUR commitment
      
      Budget approved: $2.5M USD
      Timeline approved with conditions
      Next review: Week 5 milestone
      
      @SarahChen please confirm conditions and proceed.
      ```
    - Tags Opportunity Manager
    - Clicks "Make Decision" button (opens decision modal not shown)

12. **Record Decision in System**
    - Modal opens with fields:
      * Decision: [Go ✅] [No-Go ❌]
      * Conditions: [Pre-filled from comment]
      * Authority Level: DOA-3 [Auto-filled]
      * Signature: [E-sign]
    - Clicks "Submit Decision"
    - System updates opportunity stage to "Development - Approved with Conditions"
    - Sends notifications to all stakeholders

**Time to Complete**: 20-25 minutes (thorough review)
**Tab Switches**: 6 tabs reviewed (Overview, Finances, Stakeholders, Timeline, AI Insights, Geography, Activity)
**Cognitive Load**: Medium - focused review per section, but must remember details across tabs

**Decision Benefits**:
- AI Insights tab consolidated all analytical information
- Clear risk assessment with supporting data
- Similar opportunities provided confidence
- Tab structure allowed systematic review
- Could return to any section for clarification

**Decision Challenges**:
- Had to remember financial details when reviewing risks
- No side-by-side comparison available
- Needed to switch tabs to correlate information
- Comment in Activity tab separated from context being discussed

---

## Implementation Considerations

### Technical Requirements
- **Tab State Management**: Remember last active tab, restore on return
- **Lazy Loading**: Load tab content only when accessed
- **Badge System**: Real-time notification badges for AI suggestions and updates
- **Auto-save**: Per-tab auto-save with conflict resolution
- **Keyboard Shortcuts**: Alt+1 through Alt+9 for tab navigation

### Accessibility
- **ARIA Tabs Pattern**: Proper role="tablist", role="tab", role="tabpanel"
- **Keyboard Navigation**: Arrow keys to switch tabs, Tab key to navigate within
- **Focus Management**: Restore focus to last element when returning to tab
- **Screen Reader Announcements**: Announce tab changes and badge notifications

### Responsive Considerations
- **Desktop (≥1280px)**: Full tab bar as shown
- **Tablet (768-1279px)**: Horizontal scrolling tab bar with arrows
- **Mobile (<768px)**: Tabs collapse to dropdown menu, one section at a time

### Performance Optimization
- **Tab Content Caching**: Keep last 3 accessed tabs in memory
- **Preloading**: Preload next likely tab based on user behavior patterns
- **Debounced Auto-save**: 3-second delay per tab to reduce server calls
- **Progressive Enhancement**: Basic forms work without JavaScript, AI features enhance

---

## Best Suited For

This tabbed organization approach works best when:

✅ **Users need to focus** on specific aspects without distraction
✅ **Opportunities have many sections** that would create very long single-page views
✅ **Team collaboration** where different members work on different aspects
✅ **Step-by-step workflows** where users complete one section before moving to next
✅ **Performance is critical** and loading all data at once would be slow
✅ **Mobile users** need to access the system (better responsive design)
✅ **Information architecture is clear** and users understand where to find things

This option provides excellent focus and organization but requires users to understand the tab structure and be comfortable with some context switching between different aspects of the opportunity.

