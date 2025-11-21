# UI Option 1: Unified Dashboard View

## Overview
A comprehensive single-page dashboard that presents all opportunity information in a structured, at-a-glance format. This approach emphasizes visibility and quick access to all aspects of the opportunity without navigation between screens.

## ASCII Layout Diagram

```
┌─────────────────────────────────────────────────────────────────────────────────────────────────┐
│ OPPORTUNITY+ │ [Draft] Opportunity Name [OPP-12345]                    Status: Draft  Stage: ▼ │
│ ◀ Back to List │ Partner Ref: ABC-2024-001                              👤 Manager  📋 Actions  │
├─────────────────────────────────────────────────────────────────────────────────────────────────┤
│                                                                                                   │
│  ┌─────────────────────────────────────────────────┬────────────────────────────────────────┐   │
│  │ 🤖 AI ASSISTANT PANEL                           │  📊 QUICK STATS                        │   │
│  │ ┌────────────────────────────────────────────┐  │  💰 $2.5M USD Budget                   │   │
│  │ │ • Missing: Target Signing Date            │  │  📅 Target: Q3 2025                    │   │
│  │ │ • Suggested: 3 similar opportunities       │  │  🌍 3 Countries                        │   │
│  │ │ • Analysis: High complexity detected       │  │  👥 5 Stakeholders                     │   │
│  │ │ • Risks: 4 contextual risks identified     │  │  ⚠️  2 Risks flagged                   │   │
│  │ └────────────────────────────────────────────┘  │  📄 8 Documents                        │   │
│  │ [View Full Analysis] [Generate Draft Budget]    │                                        │   │
│  └─────────────────────────────────────────────────┴────────────────────────────────────────┘   │
│                                                                                                   │
│  ┌────────────────────────────────────────────────────────────────────────────────────────────┐ │
│  │ 🎯 WHAT - Opportunity Overview                                       [✏️ Edit] [🔍 Detail] │ │
│  ├────────────────────────────────────────────────────────────────────────────────────────────┤ │
│  │ Description: Brief narrative summary of the initiative...                                  │ │
│  │                                                                                            │ │
│  │ Responsible Org Unit: [Regional Office - Asia Pacific        ▼]                           │ │
│  │ Partnership Agreement: [Select existing or upload new         ▼]                          │ │
│  │ Proposed Initiative Type: [Project  ○ Programme  ○ Portfolio  ○]                         │ │
│  │ Initiative Budget (USD): [$2,500,000.00              ]                                    │ │
│  │                                                                                            │ │
│  │ 📦 DELIVERABLES (3)                                                          [+ Add New]   │ │
│  │ ┌──────────────────────────────────────────────────────────────────────────────────────┐  │ │
│  │ │ 1. Infrastructure Development - Construction of 50 water points                      │  │ │
│  │ │    Service Line: Infrastructure / Water & Sanitation                   [✏️] [🗑️]    │  │ │
│  │ ├──────────────────────────────────────────────────────────────────────────────────────┤  │ │
│  │ │ 2. Capacity Building - Training of 200 community technicians           [✏️] [🗑️]    │  │ │
│  │ ├──────────────────────────────────────────────────────────────────────────────────────┤  │ │
│  │ │ 3. Monitoring & Evaluation - Quarterly assessment system               [✏️] [🗑️]    │  │ │
│  │ └──────────────────────────────────────────────────────────────────────────────────────┘  │ │
│  └────────────────────────────────────────────────────────────────────────────────────────────┘ │
│                                                                                                   │
│  ┌───────────────────────────────┬────────────────────────────────────────────────────────────┐ │
│  │ 👥 WHO - Partners & People    │ 💡 WHY - Impact & Alignment                               │ │
│  ├───────────────────────────────┤                                                            │ │
│  │ 💰 FUNDING PARTNERS (2)       │ 🎯 Strategic Alignment:                                    │ │
│  │ • World Bank                  │ • UNOPS Strategic Plan 2022-2025: Infrastructure          │ │
│  │   $1.8M USD (72%)  Fee: 7%    │ • Regional Strategy: Water Security                       │ │
│  │ • EU Commission               │                                                            │ │
│  │   $700K EUR (28%)  Fee: 5%    │ 🌱 SDG ALIGNMENT (3)                                       │ │
│  │ [+ Add Partner]               │ • SDG 6: Clean Water (Primary - High contribution)        │ │
│  │                               │ • SDG 13: Climate Action (Secondary)                      │ │
│  │ 🏢 CLIENT PARTNERS (1)        │ • SDG 17: Partnerships (Secondary)                        │ │
│  │ • Ministry of Water Resources │ [+ Add SDG]                                               │ │
│  │ [+ Add Partner]               │                                                            │ │
│  │                               │ 👨‍👩‍👧‍👦 BENEFICIARIES:                                             │ │
│  │ 👤 STAKEHOLDERS (5)           │ 500,000 people in rural communities                       │ │
│  │ • Sarah Chen (Opportunity Mgr)│                                                            │ │
│  │ • James Wilson (Tech Lead)    │ 📈 EXPECTED OUTCOMES:                                      │ │
│  │ • Maria Garcia (Finance)      │ • Improved access to clean water                          │ │
│  │ • External: Dr. Ahmed (Gov)   │ • Enhanced local capacity for maintenance                 │ │
│  │ • External: Lisa Park (NGO)   │ • Sustainable water management systems                    │ │
│  │ [+ Add Stakeholder]           │                                                            │ │
│  └───────────────────────────────┴────────────────────────────────────────────────────────────┘ │
│                                                                                                   │
│  ┌────────────────────────────────────────────────────────────────────────────────────────────┐ │
│  │ 📅 WHEN - Timeline & Milestones                                                [View Gantt] │ │
│  ├────────────────────────────────────────────────────────────────────────────────────────────┤ │
│  │ Target Signing Date:   [__/__/____  📅]     Target Delivery Date:  [__/__/____  📅]       │ │
│  │                                                                                            │ │
│  │ OPPORTUNITY DEVELOPMENT TIMELINE:                                                          │ │
│  │ ═══════════════════════════════════════════════════════════════════════════════            │ │
│  │ Jan 2025    Feb 2025     Mar 2025      Apr 2025      May 2025     Jun 2025                 │ │
│  │    ▼           ▼            ▼             ▼             ▼            ▼                     │ │
│  │  Kickoff   Profile &    Budget Dev    DST Review   Go/No-Go     Contract                   │ │
│  │           Analysis                                Decision       Signing                   │ │
│  │                                                                                            │ │
│  │ 🚩 KEY MILESTONES:                                                           [+ Add]        │ │
│  │ • Week 1: Project approval and kickoff                                    ✅ Complete      │ │
│  │ • Week 5: Milestone and resource planning                                 🔵 Current       │ │
│  │ • Week 9: Beta test 1.2 rollout, documentation and usability feedback     ⚪ Pending       │ │
│  │ • Week 15: Final project checkpoint and close                             ⚪ Pending       │ │
│  └────────────────────────────────────────────────────────────────────────────────────────────┘ │
│                                                                                                   │
│  ┌────────────────────────────────────────────────────────────────────────────────────────────┐ │
│  │ 🌍 WHERE - Geographic Implementation                                       [🗺️ Full Map]   │ │
│  ├────────────────────────────────────────────────────────────────────────────────────────────┤ │
│  │ COUNTRIES OF IMPLEMENTATION (3)                                            [+ Add Country]  │ │
│  │                                                                                            │ │
│  │ ┌──────────────────────────────────────────┐  ┌────────────────────────────────────────┐  │ │
│  │ │  🗺️                                      │  │ 1. 🇧🇩 Bangladesh                      │  │ │
│  │ │       INTERACTIVE MAP                    │  │    Areas: Chittagong, Sylhet           │  │ │
│  │ │                                          │  │    Context: Fragile state ⚠️           │  │ │
│  │ │    [Shows implementation countries       │  │    Risk Score: 6.2/10                  │  │ │
│  │ │     with markers and regions]            │  │                                        │  │ │
│  │ │                                          │  │ 2. 🇳🇵 Nepal                           │  │ │
│  │ │                                          │  │    Areas: Kathmandu Valley             │  │ │
│  │ │                                          │  │    Context: Post-disaster recovery     │  │ │
│  │ │                                          │  │                                        │  │ │
│  │ │         📍 📍 📍                         │  │ 3. 🇲🇲 Myanmar                         │  │ │
│  │ │                                          │  │    Areas: Yangon, Mandalay             │  │ │
│  │ │                                          │  │    Context: High complexity ⚠️         │  │ │
│  │ └──────────────────────────────────────────┘  └────────────────────────────────────────┘  │ │
│  └────────────────────────────────────────────────────────────────────────────────────────────┘ │
│                                                                                                   │
│  ┌─────────────────────────────────────────────┬──────────────────────────────────────────────┐ │
│  │ 📄 DOCUMENTS & ARTIFACTS                    │ 🔗 RELATED ENTITIES                          │ │
│  ├─────────────────────────────────────────────┤                                              │ │
│  │ 📤 DROP ZONE - Upload Documents             │ 🤝 INTERACTIONS (12)                         │ │
│  │ ┌─────────────────────────────────────────┐ │ • Meeting with World Bank - Jan 15           │ │
│  │ │   Drag & drop files here or click       │ │ • Call with Ministry - Jan 20                │ │
│  │ │   to browse                             │ │ • Email thread with EU - Jan 22              │ │
│  │ │                                         │ │ [View All]                                   │ │
│  │ │   🤖 AI will extract key information    │ │                                              │ │
│  │ └─────────────────────────────────────────┘ │ 👥 CONTACTS (8)                              │ │
│  │                                             │ • John Smith - World Bank                    │ │
│  │ 📁 USER-PROVIDED DOCUMENTS (5)              │ • Maria Lopez - Ministry Official            │ │
│  │ • Concept Note v2.pdf                 [👁️]  │ • Ahmed Hassan - Local NGO                   │ │
│  │ • Partner Correspondence.docx         [👁️]  │ [View All]                                   │ │
│  │ • Budget Template.xlsx                [👁️]  │                                              │ │
│  │ • Risk Assessment.pdf                 [👁️]  │ 🏢 PARTNERS (3)                              │ │
│  │ • Strategic Plan 2024.pdf             [👁️]  │ • World Bank                                 │ │
│  │                                             │ • European Commission                        │ │
│  │ 📊 SYSTEM-GENERATED ARTIFACTS (3)           │ • Ministry of Water Resources                │ │
│  │ • DST Profile Report                  [👁️]  │ [View All]                                   │ │
│  │ • Draft Budget v1.0                   [👁️]  │                                              │ │
│  │ • Draft Risk Register                 [👁️]  │ 📋 RELATED PROJECTS (2)                      │ │
│  │ • Opportunity Statement (Draft)       [👁️]  │ • Water Infrastructure Phase 1               │ │
│  │                                             │ • Community Development Program              │ │
│  └─────────────────────────────────────────────┴──────────────────────────────────────────────┘ │
│                                                                                                   │
│  ┌────────────────────────────────────────────────────────────────────────────────────────────┐ │
│  │ 🎲 DST INSIGHTS & RECOMMENDATIONS                              [🔄 Refresh Analysis]        │ │
│  ├────────────────────────────────────────────────────────────────────────────────────────────┤ │
│  │ PROFILE SCORE: 7.2/10 (Medium-High Complexity)                Last Updated: 2 hours ago    │ │
│  │                                                                                            │ │
│  │ ⚠️  RISKS IDENTIFIED (4)                    ✅ OPPORTUNITIES (3)                           │ │
│  │ • Political instability in Myanmar          • Leverage existing partnership with World Bank│ │
│  │ • Limited local technical capacity          • Strong government commitment                 │ │
│  │ • Monsoon season constraints                • Proven technology solutions available        │ │
│  │ • Currency fluctuation risk (EUR)           [View All]                                     │ │
│  │ [Add to Risk Register]                                                                     │ │
│  │                                                                                            │ │
│  │ 🔍 SIMILAR OPPORTUNITIES (3)                💡 RECOMMENDATIONS                             │ │
│  │ • Water Infrastructure - Nepal 2023         • Recommend: Add gender advisor to team        │ │
│  │   Relevance: 89% | Budget: $2.1M           • Consider: Split into 2 phases               │ │
│  │ • Rural Water Supply - Bangladesh 2022      • Suggested: Early environmental assessment    │ │
│  │   Relevance: 85% | Budget: $3.2M           [View All] [Accept] [Dismiss]                 │ │
│  │ • Community WASH - Myanmar 2021                                                            │ │
│  │   Relevance: 82% | Budget: $1.8M                                                           │ │
│  │ [View Details]                                                                             │ │
│  └────────────────────────────────────────────────────────────────────────────────────────────┘ │
│                                                                                                   │
│  ┌────────────────────────────────────────────────────────────────────────────────────────────┐ │
│  │ 💬 COLLABORATION & COMMENTS                                               [Filter: All ▼] │ │
│  ├────────────────────────────────────────────────────────────────────────────────────────────┤ │
│  │ 👤 Sarah Chen (You) - 2 hours ago                                                          │ │
│  │ "Updated budget estimates based on latest partner feedback. Please review new deliverable  │ │
│  │ structure." @JamesWilson @MariaGarcia                                                      │ │
│  │ ├─ 👤 James Wilson - 1 hour ago: "Looks good, but we may need to adjust timeline"         │ │
│  │ └─ 👤 Maria Garcia - 30 min ago: "Fee calculations need verification"                     │ │
│  │                                                                                            │ │
│  │ 🤖 AI Assistant - 3 hours ago                                                              │ │
│  │ "Analysis complete: 4 new risks identified from country context data. Recommend review."   │ │
│  │                                                                                            │ │
│  │ [💬 Add Comment...]                                                   [@Mention] [📎 Attach]│ │
│  └────────────────────────────────────────────────────────────────────────────────────────────┘ │
│                                                                                                   │
│  ┌────────────────────────────────────────────────────────────────────────────────────────────┐ │
│  │                                            ACTION BAR                                       │ │
│  │ [💾 Save Draft] [📤 Submit for Review] [🔄 Generate Artifacts] [📋 Export] [⚙️ Settings]    │ │
│  └────────────────────────────────────────────────────────────────────────────────────────────┘ │
└─────────────────────────────────────────────────────────────────────────────────────────────────┘
```

## Layout & UX Notes

### Visual Hierarchy
- **Top Priority**: AI Assistant panel and Quick Stats provide immediate situational awareness
- **Information Architecture**: Organized by the 5W framework (What, Who, Why, When, Where)
- **Progressive Disclosure**: Core information visible, details accessible via expand/edit actions
- **Context Awareness**: Related entities and documents always visible in right sidebar

### Key UX Features

1. **AI Integration Front and Center**
   - Prominent AI assistant panel at top showing active suggestions and analysis
   - Real-time feedback on missing information and recommendations
   - One-click actions to accept AI suggestions or generate artifacts

2. **Continuous Scrolling**
   - Single-page design eliminates navigation friction
   - Users can see relationships between sections naturally
   - Browser search (Ctrl+F) works across all opportunity content

3. **Inline Editing**
   - Each section has contextual edit buttons
   - Changes auto-save with clear indicators
   - No mode switching between view and edit

4. **Visual Emphasis**
   - Icons and emojis for quick section identification
   - Color coding for status, priorities, and risk levels
   - Progress indicators for completion status

5. **Collaboration Features**
   - Real-time comment thread at bottom
   - @mentions for stakeholder notifications
   - Activity stream showing what's changed

## Pros

✅ **Complete Context at a Glance**
- All information visible without navigation
- Easy to understand relationships between components
- Reduces cognitive load of remembering where information is located

✅ **Efficient for Experienced Users**
- Power users can scan entire opportunity quickly
- Browser search works across all content
- Single-screen printing/PDF export is straightforward

✅ **Strong AI Integration**
- AI insights constantly visible, not hidden in a menu
- Recommendations shown in context of relevant data
- Easy to act on AI suggestions immediately

✅ **Natural Information Flow**
- Follows logical 5W structure (What, Who, Why, When, Where)
- Related information grouped naturally
- Documents and related entities always accessible

✅ **Collaboration-Friendly**
- Comment stream shows full conversation history
- Easy to reference specific sections in discussions
- Real-time updates visible to all users

## Cons

❌ **Information Overload**
- Can be overwhelming for new users or simple opportunities
- Scrolling required to access lower sections
- May feel cluttered with all information visible

❌ **Performance Concerns**
- Loading all data at once may be slow for large opportunities
- Real-time updates across entire page could be resource-intensive
- May not scale well on slower devices or connections

❌ **Limited Focus**
- Hard to concentrate on specific tasks with everything visible
- No guided workflow for new opportunities
- Users might miss important fields among the clutter

❌ **Mobile/Tablet Challenges**
- Difficult to adapt to smaller screens
- Horizontal space requirements not mobile-friendly
- Touch interactions challenging with dense information

❌ **Vertical Space**
- Requires significant scrolling for complete review
- Critical information at bottom may be missed
- Difficult to compare sections that are far apart

## Persona Task Workflows

### Opportunity Manager: Creating a New Opportunity

1. **Initial Setup**
   - Clicks "Create New Opportunity" from list view
   - Sees AI panel prompting to upload documents or start manual entry
   - Drags concept note into document drop zone

2. **AI-Assisted Data Entry**
   - AI extracts information and highlights suggested fields in What section
   - Reviews and confirms: Name, Description, Budget estimate
   - AI suggests relevant SDGs based on description - accepts 2, adds 1 manually

3. **Building Team & Partners**
   - Scrolls to Who section
   - Adds funding partners: World Bank ($1.8M), EU Commission ($700K)
   - Assigns internal stakeholders from dropdown (auto-suggests based on org unit)
   - AI flags missing client partner - adds Ministry from partner directory

4. **Timeline & Geography**
   - Scrolls to When section, enters target signing date (Q3 2025)
   - AI generates suggested milestone timeline based on similar opportunities
   - Moves to Where section, adds 3 countries with specific districts
   - AI immediately flags Myanmar as high-risk context

5. **Review & Submit**
   - Scrolls to top to review AI recommendations
   - Clicks "Generate Draft Budget" - AI creates artifact in seconds
   - Reviews DST insights showing 7.2/10 complexity score
   - Adds comment @mentioning team: "Please review draft budget"
   - Clicks "Submit for Review" to send to Opportunity Authority

**Time to Complete**: 25-30 minutes (with AI assistance)
**Scroll Actions**: ~15 scroll movements to navigate all sections
**Cognitive Load**: Medium - all information visible but requires scrolling

---

### Team Member: Updating Deliverables

1. **Navigate to Opportunity**
   - Opens opportunity from notification/task list
   - Page loads showing full opportunity (takes 2-3 seconds for large opportunity)

2. **Locate Section**
   - Uses browser Ctrl+F to search "deliverables" 
   - OR scrolls to What section (3 scroll actions)

3. **Edit Deliverables**
   - Clicks "Edit" button in Deliverables subsection
   - Updates deliverable #2 description inline
   - Adds new deliverable #4 using "+ Add New" button
   - AI suggests Service Line classification - accepts

4. **Verify & Communicate**
   - Scrolls to bottom to add comment: "Deliverables updated per partner request"
   - @mentions Opportunity Manager
   - Changes auto-save with green checkmark indicator
   - Closes tab

**Time to Complete**: 5-7 minutes
**Scroll Actions**: 3-5 scrolls to reach section and return to comments
**Cognitive Load**: Low - familiar with layout, focused task

---

### System / AI: Providing Intelligent Assistance

1. **Continuous Analysis**
   - Monitors all field changes in real-time
   - Detects missing required field (Target Signing Date)
   - Updates AI panel: "Missing: Target Signing Date"

2. **Contextual Recommendations**
   - User adds "Bangladesh" as implementation country
   - AI retrieves country profile data automatically
   - Flags in AI panel: "Context: Bangladesh - Fragile state ⚠️"
   - Adds automatic recommendation: "Consider: Early risk assessment required"

3. **Similarity Detection**
   - User completes What section with water infrastructure focus
   - AI searches knowledge base for similar opportunities
   - Displays 3 most relevant past opportunities with relevance scores
   - Extracts lessons learned from similar projects

4. **Proactive Artifact Generation**
   - User clicks "Generate Draft Budget"
   - AI uses deliverables, partners, timeline, and country cost data
   - Creates high-level budget artifact in system-generated documents
   - Adds comment notification: "Draft Budget v1.0 generated - please review"

5. **Risk Intelligence**
   - Analyzes combination: Myanmar location + infrastructure scope + $2.5M budget
   - Identifies 4 contextual risks from knowledge base
   - Surfaces in DST Insights section with actionable buttons
   - Suggests: "Add to Risk Register" for each identified risk

**AI Update Frequency**: Real-time for field changes, 5-minute batch analysis for recommendations
**Processing Load**: Distributed - simple validations immediate, complex analysis queued
**User Visibility**: Always-on AI panel shows current analysis status

---

### Opportunity Authority: Making Go/No-Go Decision

1. **Review Request**
   - Receives notification: "Opportunity OPP-12345 submitted for review"
   - Clicks link to open opportunity in full dashboard view

2. **Rapid Assessment**
   - Scans Quick Stats panel: Budget $2.5M, 3 countries, 2 risks flagged
   - Reviews AI assistant summary at top
   - Notes DST score: 7.2/10 (Medium-High Complexity)

3. **Section-by-Section Review**
   - **What**: Verifies deliverables are clearly defined and achievable
   - **Who**: Confirms funding commitments and stakeholder roles
   - **Why**: Reviews SDG alignment and strategic fit
   - **When**: Assesses timeline feasibility
   - **Where**: Notes high-risk Myanmar context - reads country details
   - **Documents**: Opens Concept Note artifact to review partner-facing document

4. **DST Insights Deep Dive**
   - Scrolls to DST section
   - Reviews 4 identified risks - considers severity
   - Examines 3 similar opportunities - checks outcomes
   - Notes AI recommendation: "Add gender advisor" - agrees this is necessary

5. **Collaboration**
   - Scrolls to bottom comments section
   - Adds comment: "Approved with conditions: 1) Add gender advisor to team, 2) Complete environmental assessment before Week 5, 3) Develop mitigation plan for Myanmar political risks"
   - @mentions Opportunity Manager

6. **Decision**
   - Scrolls back to top
   - Clicks "Submit for Review" which opens decision modal (not shown in diagram)
   - Selects: Go ✅ with conditions
   - Modal auto-populates decision rationale from comment
   - Submits decision - triggers workflow to next stage

**Time to Complete**: 15-20 minutes (thorough review)
**Scroll Actions**: 20-25 scrolls (full opportunity review)
**Cognitive Load**: High - must synthesize large amount of information across sections

**Decision Factors**:
- Quick Stats provided fast overview
- AI insights reduced analysis time
- Similar opportunities gave confidence
- All information available without switching screens
- Could review documents inline without losing context

---

## Implementation Considerations

### Technical Requirements
- **Lazy Loading**: Implement intersection observer for below-fold sections
- **Auto-save**: Debounced saves (2-second delay) to reduce server load
- **Real-time Updates**: WebSocket connection for collaborative editing
- **AI Processing**: Background workers for analysis to avoid blocking UI
- **Performance**: Virtual scrolling for large lists (stakeholders, documents)

### Accessibility
- **Keyboard Navigation**: Skip-to-section shortcuts (Alt+1 for What, Alt+2 for Who, etc.)
- **Screen Readers**: Proper heading hierarchy and ARIA labels
- **Focus Management**: Maintain focus position during auto-saves
- **Color Contrast**: Ensure all status indicators meet WCAG AA standards

### Responsive Considerations
- **Desktop**: Full dashboard layout as shown (minimum 1280px width)
- **Tablet**: Switch to accordion-style sections, one visible at a time
- **Mobile**: Convert to tabbed navigation (sacrifice unified view for usability)

### Data Management
- **Caching**: Cache opportunity data in browser for offline viewing
- **Conflict Resolution**: Last-write-wins with notification to other users
- **Version Control**: Track changes at field level for audit trail
- **Export**: Generate PDF preserving visual layout for sharing

---

## Best Suited For

This unified dashboard approach works best when:

✅ **Users are experienced** with the opportunity development process
✅ **Opportunities are moderately complex** (not too simple, not extremely large)
✅ **Desktop-first environment** where users have large screens
✅ **Collaboration is frequent** and users need to reference multiple sections
✅ **Quick assessment is critical** for managers and authorities
✅ **AI insights need prominence** to drive adoption of intelligent features

This option provides maximum information density and context but requires users comfortable with information-rich interfaces and willing to scroll through content to access all features.

