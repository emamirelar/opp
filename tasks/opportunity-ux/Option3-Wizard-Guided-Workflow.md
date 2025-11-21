# UI Option 3: Wizard-Guided Workflow

## Overview
A step-by-step wizard interface that guides users through opportunity creation and management in a logical sequence. This approach emphasizes guidance, completeness, and AI-assisted progressive development with clear navigation between stages.

## ASCII Layout Diagram

```
┌─────────────────────────────────────────────────────────────────────────────────────────────────┐
│ OPPORTUNITY+ WIZARD                                                                               │
│ Water Infrastructure Initiative [OPP-12345]                            Status: Draft - In Progress│
├─────────────────────────────────────────────────────────────────────────────────────────────────┤
│                                                                                                   │
│  OPPORTUNITY DEVELOPMENT WORKFLOW                                                                 │
│  ═══════════════════════════════════════════════════════════════════════════════════════════════ │
│                                                                                                   │
│  Step 1          Step 2           Step 3          Step 4         Step 5         Step 6           │
│  ═══════        ─────────       ─────────       ─────────      ─────────      ─────────          │
│  Getting        What We'll       Who's           Why This       When & Where   Review &           │
│  Started        Deliver          Involved        Matters                       Submit             │
│    ✅             ✅               🔵              ⚪             ⚪             ⚪               │
│  Complete       Complete         Current         Not Started    Not Started    Not Started        │
│                                                                                                   │
└─────────────────────────────────────────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────────────────────────────────────────┐
│                                   STEP 3: WHO'S INVOLVED                                          │
│                          Identify partners, stakeholders, and team members                        │
├─────────────────────────────────────────────────────────────────────────────────────────────────┤
│                                                                                                   │
│  ┌────────────────────────────────────────────────────────────────────────────────────────────┐ │
│  │ 🤖 AI GUIDANCE FOR THIS STEP                                          [Minimize] [Help ❓] │ │
│  ├────────────────────────────────────────────────────────────────────────────────────────────┤ │
│  │ I found 2 funding partners mentioned in your uploaded concept note:                        │ │
│  │                                                                                            │ │
│  │ ✓ World Bank - $1.8M USD mentioned on page 3                                              │ │
│  │ ✓ European Commission - €700K EUR mentioned on page 5                                     │ │
│  │                                                                                            │ │
│  │ [✅ Add Both Partners] [Review Details] [Add Manually Instead]                            │ │
│  │                                                                                            │ │
│  │ 💡 Based on similar water infrastructure projects, you may also need:                     │ │
│  │ • Technical lead with water engineering expertise                                         │ │
│  │ • Gender advisor (required for this context)                                              │ │
│  │ • Environmental specialist                                                                │ │
│  │                                                                                            │ │
│  │ [View Team Recommendations] [Skip for Now]                                                │ │
│  └────────────────────────────────────────────────────────────────────────────────────────────┘ │
│                                                                                                   │
│  ┌────────────────────────────────────────────────────────────────────────────────────────────┐ │
│  │ 3A. FUNDING PARTNERS 💰                                                                    │ │
│  │ Who is providing the financial resources for this opportunity?                            │ │
│  ├────────────────────────────────────────────────────────────────────────────────────────────┤ │
│  │                                                                                            │ │
│  │ ┌──────────────────────────────────────────────────────────────────────────────────────┐  │ │
│  │ │ Partner 1 of 2                                                 [✏️ Edit] [🗑️ Remove]  │  │ │
│  │ ├──────────────────────────────────────────────────────────────────────────────────────┤  │ │
│  │ │ Partner Name:       [🏦 World Bank                                    ▼]              │  │ │
│  │ │                     🤖 Auto-filled from document extraction                           │  │ │
│  │ │                                                                                       │  │ │
│  │ │ Funded Amount:      [$1,800,000.00              ]  Currency: [USD ▼]                 │  │ │
│  │ │ Percentage of Total: 72%                                                             │  │ │
│  │ │                                                                                       │  │ │
│  │ │ Fee Calculation:    [● Percentage  ○ Fixed Amount]                                   │  │ │
│  │ │ Fee Percentage:     [7.0  %]           Fee Amount: $126,000.00 USD                   │  │ │
│  │ │                                                                                       │  │ │
│  │ │ Partnership Agreement: [UNOPS-WorldBank-MOU-2023                ▼] [📄 View]         │  │ │
│  │ │ 🤖 Found existing partnership agreement - auto-applied standard terms                │  │ │
│  │ └──────────────────────────────────────────────────────────────────────────────────────┘  │ │
│  │                                                                                            │ │
│  │ ┌──────────────────────────────────────────────────────────────────────────────────────┐  │ │
│  │ │ Partner 2 of 2                                                 [✏️ Edit] [🗑️ Remove]  │  │ │
│  │ ├──────────────────────────────────────────────────────────────────────────────────────┤  │ │
│  │ │ Partner Name:       [🇪🇺 European Commission                          ▼]              │  │ │
│  │ │                                                                                       │  │ │
│  │ │ Funded Amount:      [€700,000.00                ]  Currency: [EUR ▼]                 │  │ │
│  │ │ Percentage of Total: 28%                                                             │  │ │
│  │ │                                                                                       │  │ │
│  │ │ Fee Calculation:    [● Percentage  ○ Fixed Amount]                                   │  │ │
│  │ │ Fee Percentage:     [5.0  %]           Fee Amount: €35,000.00 EUR                    │  │ │
│  │ │                                        💱 ~$38,500 USD (rate: 1.10)                  │  │ │
│  │ │                                                                                       │  │ │
│  │ │ Partnership Agreement: [None - New Partner                      ▼]                   │  │ │
│  │ │ ⚠️  No existing agreement found - standard terms will apply                          │  │ │
│  │ └──────────────────────────────────────────────────────────────────────────────────────┘  │ │
│  │                                                                                            │ │
│  │ [+ Add Another Funding Partner]                                                           │ │
│  │                                                                                            │ │
│  │ Total Funding: $2,500,000.00 USD (including converted amounts)                            │ │
│  │ Total Fees:    $164,500.00 USD (6.58% effective rate)                                     │ │
│  └────────────────────────────────────────────────────────────────────────────────────────────┘ │
│                                                                                                   │
│  ┌────────────────────────────────────────────────────────────────────────────────────────────┐ │
│  │ 3B. CLIENT PARTNERS 🏢                                                                     │ │
│  │ Who are the beneficiary organizations or implementing partners?                           │ │
│  ├────────────────────────────────────────────────────────────────────────────────────────────┤ │
│  │                                                                                            │ │
│  │ Client Partner 1:  [🏛️ Ministry of Water Resources - Bangladesh        ▼]                 │ │
│  │                    [📋 View Partner Profile] [📞 View Contacts]                           │ │
│  │                                                                                            │ │
│  │ [+ Add Another Client Partner]                                                            │ │
│  │                                                                                            │ │
│  │ 💡 Tip: Client partners are the organizations that will receive and benefit from the      │ │
│  │    delivered services. These are typically government ministries, NGOs, or community      │ │
│  │    organizations in the implementation countries.                                         │ │
│  └────────────────────────────────────────────────────────────────────────────────────────────┘ │
│                                                                                                   │
│  ┌────────────────────────────────────────────────────────────────────────────────────────────┐ │
│  │ 3C. TEAM & STAKEHOLDERS 👥                                                                 │ │
│  │ Who from UNOPS and externally will be involved in this opportunity?                       │ │
│  ├────────────────────────────────────────────────────────────────────────────────────────────┤ │
│  │                                                                                            │ │
│  │ ┌─────────────────────────────────────┬──────────────────────────────────────────────┐    │ │
│  │ │ 👨‍💼 INTERNAL TEAM (UNOPS)            │ 🌐 EXTERNAL STAKEHOLDERS                      │    │ │
│  │ ├─────────────────────────────────────┼──────────────────────────────────────────────┤    │ │
│  │ │                                     │                                              │    │ │
│  │ │ Opportunity Manager (Required):     │ External Stakeholder 1:                      │    │ │
│  │ │ [Sarah Chen                    ▼]   │ Name: [Dr. Ahmed Hassan           ]          │    │ │
│  │ │ Role: Partnerships Lead             │ Organization: [Ministry of Water  ]          │    │ │
│  │ │ ✅ Assigned                          │ Role: [Government Director        ▼]         │    │ │
│  │ │                                     │ Contact: [ahmed.hassan@gov.bd     ]          │    │ │
│  │ │ Technical Lead:                     │ [💾 Save] [❌ Cancel]                         │    │ │
│  │ │ [James Wilson                  ▼]   │                                              │    │ │
│  │ │ Role: Infrastructure Specialist     │ External Stakeholder 2:                      │    │ │
│  │ │ ✅ Assigned                          │ Name: [Lisa Park                  ]          │    │ │
│  │ │                                     │ Organization: [WaterAid NGO       ]          │    │ │
│  │ │ Finance Officer:                    │ Role: [NGO Director               ▼]         │    │ │
│  │ │ [Maria Garcia                  ▼]   │ Contact: [lisa.park@wateraid.org  ]          │    │ │
│  │ │ Role: Budget & Reporting            │ [💾 Save] [❌ Cancel]                         │    │ │
│  │ │ ✅ Assigned                          │                                              │    │ │
│  │ │                                     │ [+ Add Another External Stakeholder]         │    │ │
│  │ │ 🤖 SUGGESTED ADDITIONS:             │                                              │    │ │
│  │ │ ⚠️  Gender Advisor (Recommended)    │                                              │    │ │
│  │ │    Required for this context        │                                              │    │ │
│  │ │    [🔍 Find Available Advisor]      │                                              │    │ │
│  │ │                                     │                                              │    │ │
│  │ │ ⚠️  Environmental Specialist        │                                              │    │ │
│  │ │    Recommended for infrastructure   │                                              │    │ │
│  │ │    [🔍 Find Available Specialist]   │                                              │    │ │
│  │ │                                     │                                              │    │ │
│  │ │ [+ Add Team Member]                 │                                              │    │ │
│  │ └─────────────────────────────────────┴──────────────────────────────────────────────┘    │ │
│  └────────────────────────────────────────────────────────────────────────────────────────────┘ │
│                                                                                                   │
│  ┌────────────────────────────────────────────────────────────────────────────────────────────┐ │
│  │ ✅ STEP COMPLETION CHECK                                                                   │ │
│  ├────────────────────────────────────────────────────────────────────────────────────────────┤ │
│  │ ✅ At least one funding partner added                                                      │ │
│  │ ✅ At least one client partner added                                                       │ │
│  │ ✅ Opportunity Manager assigned                                                            │ │
│  │ ⚠️  Recommended: Consider adding gender advisor (not required to proceed)                 │ │
│  │                                                                                            │ │
│  │ You can proceed to the next step or continue refining your team composition.              │ │
│  └────────────────────────────────────────────────────────────────────────────────────────────┘ │
│                                                                                                   │
│  ┌────────────────────────────────────────────────────────────────────────────────────────────┐ │
│  │ 💬 NOTES & QUESTIONS                                                    [Expand] [Collapse]│ │
│  ├────────────────────────────────────────────────────────────────────────────────────────────┤ │
│  │ [Leave notes or questions for reviewers or team members...]                               │ │
│  │                                                                                            │ │
│  │ Recent Comments:                                                                           │ │
│  │ • Sarah Chen: "Confirmed World Bank commitment via email 2024-01-15"                      │ │
│  │ • James Wilson: "Still sourcing gender advisor - timeline may slip"                       │ │
│  └────────────────────────────────────────────────────────────────────────────────────────────┘ │
│                                                                                                   │
│  ┌────────────────────────────────────────────────────────────────────────────────────────────┐ │
│  │                                        NAVIGATION                                           │ │
│  ├────────────────────────────────────────────────────────────────────────────────────────────┤ │
│  │ [◀ Previous: What We'll Deliver]  [💾 Save Draft]  [Exit Wizard]  [Next: Why This Matters ▶]│ │
│  │                                                                                             │ │
│  │ Or jump to any completed step: [1️⃣ Getting Started] [2️⃣ What We'll Deliver]               │ │
│  └────────────────────────────────────────────────────────────────────────────────────────────┘ │
└─────────────────────────────────────────────────────────────────────────────────────────────────┘
```

## Step-by-Step Flow

### Step 1: Getting Started
```
┌───────────────────────────────────────────────────────────────────┐
│ STEP 1: GETTING STARTED                                           │
│ Let's begin by understanding what you know about this opportunity │
├───────────────────────────────────────────────────────────────────┤
│                                                                   │
│ 🤖 How would you like to start?                                  │
│                                                                   │
│ ┌─────────────────────┐  ┌─────────────────────┐                │
│ │  📄 Upload Documents│  │  ✍️ Enter Manually  │                │
│ │                     │  │                     │                │
│ │  I have concept     │  │  I'll enter         │                │
│ │  notes, proposals,  │  │  information as     │                │
│ │  or other documents │  │  I go               │                │
│ │                     │  │                     │                │
│ │  [Start Here]       │  │  [Start Here]       │                │
│ └─────────────────────┘  └─────────────────────┘                │
│                                                                   │
│ 💡 Recommended: Upload documents first! I can extract key         │
│    information and save you time.                                │
│                                                                   │
│ [Skip This - I'll Decide Later]                                  │
└───────────────────────────────────────────────────────────────────┘

IF UPLOAD CHOSEN:
┌───────────────────────────────────────────────────────────────────┐
│ 📤 DOCUMENT UPLOAD ZONE                                           │
│ ┌───────────────────────────────────────────────────────────────┐│
│ │                                                               ││
│ │   Drag and drop files here or click to browse                ││
│ │                                                               ││
│ │   Accepted: PDF, DOCX, XLSX, TXT, PPT                        ││
│ │   Max size: 50MB per file                                    ││
│ │                                                               ││
│ └───────────────────────────────────────────────────────────────┘│
│                                                                   │
│ 🤖 AI PROCESSING STATUS:                                          │
│ ✅ Concept_Note_v2.pdf - Processed (15 seconds)                  │
│    • Found: Opportunity name, description, 2 partners            │
│    • Found: Budget estimate, 3 deliverables                      │
│    • Found: Implementation countries                             │
│    [Review Extracted Data]                                       │
│                                                                   │
│ ⏳ Budget_Template.xlsx - Processing...                          │
│                                                                   │
│ [Continue to Next Step] [Upload More Documents]                  │
└───────────────────────────────────────────────────────────────────┘
```

### Step 2: What We'll Deliver
```
┌───────────────────────────────────────────────────────────────────┐
│ STEP 2: WHAT WE'LL DELIVER                                        │
│ Define the core elements of this opportunity                      │
├───────────────────────────────────────────────────────────────────┤
│                                                                   │
│ 2A. BASIC INFORMATION                                             │
│ Opportunity Name: [Water Infrastructure Initiative          ]     │
│ Partner Reference: [ABC-2024-001                            ]     │
│                                                                   │
│ Description: [Large text area with AI-suggested content...]      │
│                                                                   │
│ Responsible Org Unit: [Regional Office - Asia Pacific      ▼]    │
│ Initiative Type: [● Project  ○ Programme  ○ Portfolio]           │
│ Budget Estimate: [$2,500,000.00         ]                        │
│                                                                   │
│ 2B. DELIVERABLES (What will be delivered?)                       │
│ [List of 3 deliverables with + Add button]                       │
│                                                                   │
│ ✅ Required information complete                                  │
│ [Next: Who's Involved ▶]                                          │
└───────────────────────────────────────────────────────────────────┘
```

### Step 4: Why This Matters
```
┌───────────────────────────────────────────────────────────────────┐
│ STEP 4: WHY THIS MATTERS                                          │
│ Align this opportunity with strategic goals and impact            │
├───────────────────────────────────────────────────────────────────┤
│                                                                   │
│ 4A. STRATEGIC ALIGNMENT                                           │
│ UNOPS Strategic Plan: [Infrastructure & Water Security     ▼]    │
│ Regional Strategy: [South Asia Water Security 2024         ▼]    │
│                                                                   │
│ 4B. SUSTAINABLE DEVELOPMENT GOALS (SDGs)                          │
│ 🤖 Based on your deliverables, I recommend these SDGs:           │
│ • SDG 6: Clean Water & Sanitation (Primary - High impact)        │
│ • SDG 13: Climate Action (Secondary)                             │
│ • SDG 17: Partnerships (Secondary)                               │
│ [✅ Accept All] [Review & Customize]                              │
│                                                                   │
│ 4C. EXPECTED IMPACT                                               │
│ Beneficiaries: [500,000 people in rural communities        ]     │
│ Expected Outcomes: [Text area...]                                │
│                                                                   │
│ [Next: When & Where ▶]                                            │
└───────────────────────────────────────────────────────────────────┘
```

### Step 5: When & Where
```
┌───────────────────────────────────────────────────────────────────┐
│ STEP 5: WHEN & WHERE                                              │
│ Define timeline and geographic scope                              │
├───────────────────────────────────────────────────────────────────┤
│                                                                   │
│ 5A. TIMELINE                                                      │
│ Target Signing Date: [Q3 2025              📅]  ⚠️  REQUIRED     │
│ Target Delivery Date: [Q4 2027             📅]                   │
│                                                                   │
│ 🤖 Based on similar opportunities:                                │
│ Typical development timeline: 16-20 weeks                         │
│ [Generate Suggested Milestones]                                  │
│                                                                   │
│ 5B. IMPLEMENTATION COUNTRIES                                      │
│ Country 1: [🇧🇩 Bangladesh                 ▼]                    │
│   Specific Areas: [Chittagong, Sylhet                       ]     │
│   ⚠️  AI Flag: Fragile state - high complexity context           │
│                                                                   │
│ Country 2: [🇳🇵 Nepal                      ▼]                    │
│   Specific Areas: [Kathmandu Valley                         ]     │
│                                                                   │
│ [+ Add Country]                                                   │
│                                                                   │
│ [🗺️ View on Map]                                                  │
│                                                                   │
│ [Next: Review & Submit ▶]                                         │
└───────────────────────────────────────────────────────────────────┘
```

### Step 6: Review & Submit
```
┌───────────────────────────────────────────────────────────────────┐
│ STEP 6: REVIEW & SUBMIT                                           │
│ Final review before submitting for Go/No-Go decision              │
├───────────────────────────────────────────────────────────────────┤
│                                                                   │
│ 📊 READINESS ASSESSMENT                                           │
│ ████████████████████████████░░░░ 85% Complete                    │
│                                                                   │
│ ✅ All required information provided                              │
│ ✅ 2 funding partners confirmed ($2.5M total)                     │
│ ✅ 3 implementation countries defined                             │
│ ⚠️  2 recommended actions not completed                           │
│                                                                   │
│ 🤖 AI FINAL ANALYSIS                                              │
│ Complexity Score: 7.2/10 (Medium-High)                           │
│ Risks Identified: 4 (view details)                               │
│ Recommendations: 3 (view details)                                 │
│ Similar Opportunities: 3 found for reference                      │
│                                                                   │
│ [📄 Generate Opportunity Statement]                               │
│ [📊 Generate Draft Budget]                                        │
│ [📈 View Full DST Report]                                         │
│                                                                   │
│ REVIEW BY SECTION:                                                │
│ [1️⃣ Getting Started] [2️⃣ What We'll Deliver] [3️⃣ Who's Involved] │
│ [4️⃣ Why This Matters] [5️⃣ When & Where]                          │
│                                                                   │
│ ┌───────────────────────────────────────────────────────────────┐│
│ │ 📤 READY TO SUBMIT?                                           ││
│ │                                                               ││
│ │ This opportunity will be sent to:                             ││
│ │ • Opportunity Authority: Jane Smith (DOA-3)                   ││
│ │ • For review: Go/No-Go Decision                              ││
│ │                                                               ││
│ │ Before submitting, please confirm:                            ││
│ │ ☑ All information is accurate                                ││
│ │ ☑ Required documents are attached                            ││
│ │ ☑ Team members have been notified                            ││
│ │                                                               ││
│ │ Optional message to reviewer:                                 ││
│ │ [Text area for cover message...]                             ││
│ │                                                               ││
│ │ [📤 Submit for Review] [💾 Save Draft & Exit]                 ││
│ └───────────────────────────────────────────────────────────────┘│
└───────────────────────────────────────────────────────────────────┘
```

## Layout & UX Notes

### Workflow Philosophy
- **Linear Progression**: Users move through logical steps in sequence
- **Progressive Disclosure**: Show only relevant information for current step
- **Continuous Guidance**: AI assistant provides context-aware help at each step
- **Flexible Navigation**: Can jump to any completed step, but guided forward through incomplete steps
- **Checkpoint Validation**: Each step validates before allowing progression

### Key UX Features

1. **Visual Progress Indicator**
   - Always visible at top showing all steps
   - Clear indication of current step, completed steps, and upcoming steps
   - Progress percentage within each step

2. **Context-Aware AI Assistant**
   - Changes guidance based on current step
   - Proactive suggestions and warnings
   - Document extraction and auto-population
   - One-click acceptance of AI recommendations

3. **Chunked Information**
   - Steps broken into sub-sections (3A, 3B, 3C)
   - Prevents overwhelming users with too much at once
   - Each sub-section can be completed independently

4. **Smart Validation**
   - Real-time validation as users enter data
   - Clear indication of required vs. optional fields
   - Completion checklist at end of each step
   - Can save draft and exit at any time

5. **Helpful Tips & Guidance**
   - Contextual help text in each section
   - Examples and suggestions
   - Links to relevant policies or templates
   - Recommended actions vs. required actions

## Pros

✅ **Perfect for New Users**
- Step-by-step guidance reduces confusion
- Clear indication of what's needed at each stage
- Helpful tips and examples throughout
- Reduces errors and omissions

✅ **Enforces Best Practices**
- Logical flow ensures nothing is forgotten
- AI guidance promotes quality inputs
- Validation catches issues early
- Encourages complete and thorough documentation

✅ **AI Integration Excellence**
- AI guidance highly relevant to each step
- Document extraction streamlines data entry
- Proactive recommendations at right time
- User can accept/reject suggestions easily

✅ **Clear Progress Tracking**
- Users always know where they are
- Easy to see what's left to complete
- Can save and return without losing place
- Readiness assessment before submission

✅ **Reduces Cognitive Load**
- Focus on one aspect at a time
- No need to understand entire system upfront
- Progressive learning as users advance
- Less overwhelming than full-page interfaces

✅ **Mobile-Friendly**
- Linear flow works well on small screens
- One section visible at a time
- Touch-friendly navigation buttons
- Minimal horizontal scrolling needed

## Cons

❌ **Slower for Experienced Users**
- Must click through multiple steps even if they know what to do
- Can't edit multiple sections simultaneously
- Navigation overhead for quick updates
- May feel constrained by rigid workflow

❌ **Limited Flexibility**
- Encourages linear progression, may not match all workflows
- Harder to jump around to different sections
- May force unnecessary sequence for some users
- Difficult to work on multiple steps in parallel

❌ **Context Loss**
- Can't see relationship between steps easily
- Need to navigate back to review previous entries
- Hard to compare information across steps
- No "big picture" view of entire opportunity

❌ **Collaboration Challenges**
- Multiple team members can't work on different steps easily
- May cause bottlenecks if one person is working through wizard
- Comments/collaboration features separated from content
- Harder to discuss opportunity when everyone on different steps

❌ **Repetitive for Updates**
- Must navigate to specific step to update one field
- Overhead of wizard navigation for small changes
- Not ideal for maintaining existing opportunities
- Best suited for initial creation, not ongoing management

❌ **Limited Overview**
- No single view of entire opportunity
- Must navigate through steps to review everything
- Harder to print or export complete opportunity
- Reviewers may prefer consolidated view

## Persona Task Workflows

### Opportunity Manager: Creating a New Opportunity

1. **Launch Wizard**
   - Clicks "Create New Opportunity" from list view
   - Wizard opens to **Step 1: Getting Started**
   - AI asks: "Upload documents or enter manually?"
   - Chooses "Upload Documents"

2. **Document Processing (Step 1)**
   - Drags concept note PDF into upload zone
   - Watches AI processing indicator (15 seconds)
   - AI displays extraction summary:
     * Found: Name, description, 2 partners
     * Found: Budget $2.5M, 3 deliverables
     * Found: 3 implementation countries
   - Clicks "Continue to Next Step"

3. **Basic Information (Step 2)**
   - Lands on Step 2 with pre-filled fields from AI extraction
   - Reviews and confirms: Name, Description
   - Selects Responsible Org Unit from dropdown
   - Confirms Initiative Type: Project
   - Reviews 3 deliverables (AI extracted):
     * Reads each one
     * Edits deliverable #2 description slightly
     * Accepts others as-is
   - Green checkmark shows step complete
   - Clicks "Next: Who's Involved"

4. **Stakeholders & Partners (Step 3)**
   - AI panel shows: "I found 2 funding partners in your document"
   - Clicks "Add Both Partners"
   - Reviews Partner 1 (World Bank):
     * Amount: $1.8M - correct
     * Fee: 7% - confirms
     * Partnership Agreement: Auto-selected from existing agreements
   - Reviews Partner 2 (EU Commission):
     * Amount: €700K - correct
     * Fee: 5% - enters manually
     * No existing agreement - notes warning
   - Adds Client Partner: Ministry of Water Resources
   - Assigns team members:
     * Opportunity Manager: Auto-assigned to self
     * Technical Lead: Selects James Wilson from dropdown
     * Finance Officer: Selects Maria Garcia
   - AI suggests: "Add gender advisor"
     * Notes warning but clicks "Skip for Now" (will add later)
   - Completion check shows all required items done
   - Clicks "Next: Why This Matters"

5. **Impact Alignment (Step 4)**
   - AI suggests 3 SDGs based on deliverables
   - Reviews suggestions: SDG 6, SDG 13, SDG 17
   - Clicks "Accept All"
   - Enters beneficiaries: "500,000 people in rural communities"
   - Enters expected outcomes in text area
   - Clicks "Next: When & Where"

6. **Timeline & Geography (Step 5)**
   - Enters Target Signing Date: Q3 2025
   - Enters Target Delivery Date: Q4 2027
   - Adds countries (AI pre-populated):
     * Bangladesh - confirms, adds specific areas
     * AI immediately flags: "⚠️ Fragile state - high complexity"
     * Nepal - confirms, adds areas
     * Myanmar - confirms, adds areas
   - Clicks "View on Map" to verify locations
   - Clicks "Next: Review & Submit"

7. **Final Review (Step 6)**
   - Sees readiness assessment: 85% complete
   - Reviews AI analysis: Complexity 7.2/10, 4 risks identified
   - Clicks "View Full DST Report" - opens in modal
   - Reviews risks and recommendations
   - Clicks "Generate Opportunity Statement" - AI creates draft
   - Clicks "Generate Draft Budget" - AI creates budget
   - Reviews each section using jump links (1-5)
   - Adds message to reviewer: "Ready for review. Note: Gender advisor to be assigned next week per team discussion"
   - Checks all confirmation boxes
   - Clicks "Submit for Review"
   - Success message appears, wizard closes
   - Returns to opportunity list

**Time to Complete**: 20-25 minutes (with AI assistance and document upload)
**Click Actions**: ~35 clicks (including navigation, review, and confirmation)
**Cognitive Load**: Low - guided through each step with clear instructions

**User Experience**:
- Felt guided and supported throughout
- AI extraction saved significant time
- Never confused about what to do next
- Confident all required information was provided
- Appreciated validation at each step

---

### Team Member: Updating Deliverables (Post-Creation)

1. **Access Opportunity**
   - Receives notification: "Sarah Chen mentioned you in OPP-12345"
   - Clicks link to open opportunity
   - **Challenge**: Opens to view mode, not wizard

2. **Find Edit Option**
   - Sees opportunity in read-only view
   - Clicks "Edit" button at top
   - System asks: "What would you like to edit?"
     * Option 1: Edit specific section
     * Option 2: Enter edit wizard
   - Chooses "Edit specific section"

3. **Navigate to Deliverables**
   - Modal shows all sections
   - Clicks "What We'll Deliver (Deliverables)"
   - Opens Step 2 in edit mode

4. **Update Deliverable**
   - Edits Deliverable #2 description
   - Adds new Deliverable #4
   - AI suggests service line - accepts
   - Clicks "Save Changes"
   - System asks: "Continue through wizard or exit?"
   - Chooses "Exit to view mode"

5. **Add Comment**
   - In view mode, clicks "Activity" tab
   - Adds comment: "Deliverables updated per partner feedback"
   - @mentions Sarah Chen
   - Saves and closes

**Time to Complete**: 8-12 minutes
**Click Actions**: ~12 clicks
**Cognitive Load**: Medium - had to figure out how to edit existing opportunity

**User Experience**:
- Slightly confusing for updates (wizard designed for creation)
- Appreciated ability to edit just one section
- Would prefer direct edit mode for minor changes
- Wizard navigation felt like overhead for simple task

---

### System / AI: Providing Intelligent Assistance

1. **Document Upload Analysis (Step 1)**
   - User uploads PDF concept note
   - AI triggers OCR and NLP processing
   - Extracts structured data:
     * Text analysis for opportunity name, description
     * Pattern matching for budget amounts and currencies
     * Entity recognition for organization names
     * Geographic entity extraction for countries
   - Processing time: 15-20 seconds for 15-page document
   - Returns extraction summary with confidence scores

2. **Step-Specific Guidance (Step 2)**
   - User enters Step 2 (What We'll Deliver)
   - AI panel updates to show Step 2-specific guidance
   - Monitors deliverable entries
   - When user types "water infrastructure", AI searches knowledge base
   - Suggests service line: "Infrastructure / Water & Sanitation"
   - Shows example deliverables from similar opportunities

3. **Proactive Risk Flagging (Step 5)**
   - User adds "Myanmar" to implementation countries
   - AI immediately queries country risk database
   - Retrieves: Fragile state classification, conflict risk data
   - Displays warning in real-time: "⚠️ High complexity context"
   - Adds note to Step 6 review summary
   - Prepares detailed risk analysis for DST report

4. **Recommendation Engine (Step 3)**
   - User completes funding partners and team sections
   - AI analyzes: Infrastructure deliverables + Asia region + $2.5M budget
   - Queries similar opportunities: Finds 3 matches
   - Extracts team composition from similar opportunities
   - Identifies common role: "Gender advisor present in 8/10 similar opportunities"
   - Surfaces recommendation: "Consider adding gender advisor"
   - Provides rationale and quick-add button

5. **Readiness Assessment (Step 6)**
   - User reaches final review step
   - AI compiles all entered data
   - Runs completeness check:
     * Required fields: 100% complete ✅
     * Recommended fields: 70% complete ⚠️
   - Generates complexity score based on:
     * Number of countries (3) + fragile states (1)
     * Budget size ($2.5M) + multi-donor (2)
     * Infrastructure sector + capacity building components
     * Historical complexity data from similar opportunities
   - Calculates: 7.2/10 complexity score
   - Generates final DST report
   - Shows summary in review panel

**AI Processing Strategy**:
- **Immediate feedback**: Field-level validation (< 1 second)
- **Quick analysis**: Step completion checks (1-2 seconds)
- **Moderate processing**: Document extraction (15-20 seconds)
- **Heavy analysis**: Full DST report (30-45 seconds, triggered at Step 6)
- **Background updates**: Similar opportunity search runs while user works

---

### Opportunity Authority: Making Go/No-Go Decision (Post-Wizard Submission)

1. **Review Notification**
   - Receives email: "Opportunity OPP-12345 submitted for your review"
   - Email includes AI-generated executive summary
   - Clicks link to open opportunity

2. **Initial View**
   - Opens to Summary/Overview page (NOT wizard)
   - Sees key metrics dashboard:
     * Budget: $2.5M, Countries: 3, Complexity: 7.2/10
     * Team: Sarah Chen (Manager), 3 internal, 2 external stakeholders
     * Readiness: 85%, Status: Submitted for Review
   - AI summary panel shows: "4 risks identified, 3 recommendations"

3. **Systematic Review**
   - **Option A**: Click through wizard steps in view mode
     * Step 1: Review documents uploaded
     * Step 2: Review what will be delivered
     * Step 3: Review stakeholders and partners
     * Step 4: Review impact alignment
     * Step 5: Review timeline and geography
     * Step 6: Review readiness assessment
   - **Option B**: Use tabbed interface (system switches to tab view for reviewers)
   - **Option C**: Download PDF Opportunity Statement (AI-generated)

4. **Detailed DST Analysis**
   - Clicks "View Full DST Report" button
   - Opens comprehensive analysis in modal or new page:
     * All 9 profiling dimensions
     * 4 identified risks with severity ratings
     * 3 similar opportunities with outcomes
     * 3 recommendations with rationales
   - Notes Myanmar risk (High severity)
   - Appreciates similar opportunity comparisons

5. **Decision Making**
   - Clicks "Make Decision" button
   - Decision modal opens:
     * Decision: [Go ✅] [No-Go ❌]
     * Conditions (optional text area)
     * Comments for team
     * Digital signature
   - Enters decision: "Go with conditions"
   - Enters conditions:
     * Add gender advisor before Week 3
     * Complete environmental assessment before Week 5
     * Develop Myanmar risk mitigation plan
     * Phased implementation recommended
   - Adds comment for Opportunity Manager
   - Signs and submits

6. **System Actions**
   - Opportunity status updates to "Approved - Conditional Go"
   - Notifications sent to all stakeholders
   - Conditions added to opportunity requirements checklist
   - Next workflow stage activated (Development)
   - Decision recorded in audit trail with timestamp

**Time to Complete**: 18-22 minutes (thorough review)
**Navigation Approach**: Used PDF download + DST report for efficient review
**Decision Quality**: High - all information accessible, AI insights helpful

**Reviewer Experience**:
- Appreciated AI summary for quick assessment
- PDF Opportunity Statement was most useful format
- Wizard step review felt tedious (would prefer summary view)
- DST report excellent for decision support
- Decision modal was clear and straightforward

---

## Implementation Considerations

### Technical Requirements
- **State Management**: Maintain wizard state across sessions (save/resume)
- **Draft Auto-save**: Save progress every 30 seconds automatically
- **Step Validation**: Real-time validation with clear error messages
- **Navigation Control**: Disable forward navigation until required fields complete
- **Document Processing**: Asynchronous AI extraction with progress indicators
- **Multi-modal View**: Switch between wizard (creation) and summary (review) modes

### Accessibility
- **Keyboard Navigation**: Enter/Space to advance, Escape to cancel
- **Screen Reader**: Announce step changes and progress updates
- **Focus Management**: Maintain logical focus order, restore on step change
- **ARIA Landmarks**: Clear step regions and navigation structure
- **Progress Indicators**: Text alternatives for visual progress bars

### Responsive Considerations
- **Desktop**: Full wizard layout as shown
- **Tablet**: Slightly compressed, single-column sub-sections
- **Mobile**: One sub-section visible at a time, swipe navigation
- **Step Indicator**: Collapses to "Step X of 6" on small screens

### Post-Creation Editing
**Challenge**: Wizard designed for linear creation, not flexible editing
**Solutions**:
1. **Option A**: Provide "Quick Edit" mode bypassing wizard
2. **Option B**: Allow jump to any step directly from view mode
3. **Option C**: Switch to different UI mode (tabs or full dashboard) after creation
4. **Recommended**: Hybrid approach - wizard for creation, tabs for editing

---

## Best Suited For

This wizard-guided workflow approach works best when:

✅ **Users are new or infrequent** - Need guidance and hand-holding
✅ **Consistency is critical** - Want to ensure complete, high-quality submissions
✅ **AI assistance is core feature** - Want to showcase AI capabilities prominently
✅ **Process has clear sequence** - Logical order exists (What → Who → Why → When)
✅ **Validation is important** - Want to catch errors and omissions early
✅ **Mobile users** need to create opportunities on tablets/phones
✅ **Training burden should be minimal** - Self-explanatory interface reduces training needs
✅ **Quality over speed** - Willing to trade some efficiency for thoroughness

**Not ideal when**:
❌ Users are experts who know exactly what they need
❌ Opportunities need frequent updates and edits
❌ Flexibility and non-linear workflows are required
❌ Multiple team members need to collaborate simultaneously
❌ Speed is priority over completeness
❌ Users need to see "big picture" view regularly

This option provides the most structured and guided experience with excellent AI integration, but may feel restrictive to experienced users or for ongoing opportunity management tasks.

---

## Summary Comparison

| Aspect | Option 1: Dashboard | Option 2: Tabs | Option 3: Wizard |
|--------|-------------------|---------------|------------------|
| **Best For** | Experienced users | Focused work | New users |
| **Information Density** | High | Medium | Low |
| **Navigation Clicks** | Low (scrolling) | Medium (tabs) | High (steps) |
| **Mobile Friendly** | Poor | Good | Excellent |
| **AI Visibility** | Always prominent | Contextual | Step-specific |
| **Collaboration** | Excellent | Good | Limited |
| **Learning Curve** | Steep | Moderate | Gentle |
| **Creation Speed** | Fast (experts) | Medium | Slower (guided) |
| **Completeness** | User-dependent | Good | Enforced |
| **Flexibility** | High | Medium | Low |
| **Big Picture View** | Excellent | Limited | None |


