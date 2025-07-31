# Frontend UI Documentation Guide

🎨 **Complete Guide to Documenting Angular Components for AI Assistant**

This guide shows you how to document every Angular component in your application to provide rich contextual help through your AI assistant.

## 🎯 Overview

Your AI assistant now supports both:
- **Backend API tools** (existing) - what the AI can *do* via API calls
- **Frontend UI guidance** (new) - how to *help users* navigate and use the interface

## 📁 File Structure

After documentation, you'll have the new organized structure:

```
UNOPS.PAO.AIService/config/tools/
├── endpoints/               # Backend API endpoints
│   ├── partner-tools.json
│   ├── contact-tools.json
│   ├── interaction-tools.json
│   └── ... (all backend API tools)
├── ui/                      # Frontend UI guidance  
│   ├── partner-ui.json
│   ├── contact-ui.json
│   ├── interaction-ui.json
│   └── ... (all frontend UI tools)
└── ...
```

## 🚀 Quick Start

### Step 1: Document Your Components

Add JSDoc comments to your Angular components:

```typescript
/**
 * @uiEntity Partner
 * @route /partnerships/partners
 * @description Browse and manage partner organizations with search, filtering, and CRUD operations
 * @capabilities search_partners, create_partner, edit_partner, filter_partners, export_partners
 * @synonyms organization, collaborator, entity, associate, vendor
 * @mandatoryFields name, partnerType, status, partnerOfficeId
 * @help_when_stuck Use the search bar to find partners by name. Click + to create new partners if you have permissions.
 * @common_tasks
 *   - Finding a partner: Use the search bar at the top
 *   - Creating a partner: Click 'Create Partner' button (requires permissions)
 *   - Editing a partner: Click on any partner row to open details
 * @tabs Details:/partnerships/partners/:id, Contacts:/partnerships/partners/:id/contacts
 */
@Component({
  selector: 'app-partner',
  // ... rest of component
})
export class PartnerComponent {
  // ... component code
}
```

### Step 2: Generate UI Guidance

Run the generator:

```bash
# Windows
cd ToolsJsonGenerator
generate_frontend_tools.bat "../UNOPS.PAO.ClientApp"

# PowerShell
.\generate_frontend_tools.ps1 -AngularProject "../UNOPS.PAO.ClientApp"
```

### Step 3: Your AI Assistant Now Knows the UI

Your AI can now help users with:
- "How do I create a partner?"
- "What can I do on this page?"
- "I'm stuck on the partner page, help me"
- "Show me all partner-related features"

## 📝 JSDoc Tags Reference

### Required Tags

#### `@uiEntity`
Identifies which entity this component manages.

```typescript
/**
 * @uiEntity Partner
 */
```

**Must match your existing backend entity names for consistency.**

#### `@route` 
The primary route/URL for this component.

```typescript
/**
 * @route /partnerships/partners
 * @route /partnerships/partners/:id  // For detail pages
 * @route Modal dialog (no direct route)  // For modal components
 */
```

#### `@description`
Clear explanation of what this component/page does.

```typescript
/**
 * @description Browse and manage partner organizations with comprehensive search, filtering, and CRUD operations. Central hub for all partner-related activities.
 */
```

### Capability Tags

#### `@capabilities`
List of actions users can perform on this page.

```typescript
/**
 * @capabilities search_partners, filter_partners, create_partner, edit_partner, delete_partner, export_partners, import_partners, bulk_operations
 */
```

#### `@synonyms`
Alternative terms users might use for this entity.

```typescript
/**
 * @synonyms organization, collaborator, entity, associate, vendor, supplier, contractor
 */
```

#### `@mandatoryFields`
Required fields when creating this entity.

```typescript
/**
 * @mandatoryFields name, partnerType, status, partnerOfficeId
 */
```

### Help & Guidance Tags

#### `@help_when_stuck`
What to tell users when they're confused on this page.

```typescript
/**
 * @help_when_stuck Use the search bar to find specific partners by name, type, or location. Click the + button to create new partners if you have permissions. Use filters to narrow down results by partner type, status, or organizational unit.
 */
```

#### `@common_tasks`
Step-by-step instructions for frequent user tasks.

```typescript
/**
 * @common_tasks
 *   - Finding a partner: Use the global search bar or entity-specific filters
 *   - Creating a partner: Click 'Create Partner' button (requires PARTNER_CREATE permission)
 *   - Editing a partner: Click on any partner row to open details, then click Edit
 *   - Filtering partners: Use the advanced search and filter options in the left panel
 *   - Exporting data: Use the Export button to download partner lists in Excel format
 */
```

#### `@tabs`
For components with tab navigation.

```typescript
/**
 * @tabs Details:/partnerships/partners/:id, Contacts:/partnerships/partners/:id/contacts, Interactions:/partnerships/partners/:id/interactions, Data:/partnerships/partners/:id/data
 */
```

## 🔲 Component Types & Examples

### 1. List/Browse Pages

Components that show lists of entities (partners, contacts, etc.).

```typescript
/**
 * @uiEntity Partner
 * @route /partnerships/partners
 * @description Browse and manage partner organizations with comprehensive search, filtering, and CRUD operations. Central hub for all partner-related activities.
 * @capabilities search_partners, filter_partners, create_partner, edit_partner, delete_partner, export_partners, import_partners, bulk_operations
 * @synonyms organization, collaborator, entity, associate, vendor, supplier, contractor
 * @mandatoryFields name, partnerType, status, partnerOfficeId
 * @help_when_stuck Use the search bar to find specific partners by name, type, or location. Click the + button to create new partners if you have permissions. Use filters to narrow down results by partner type, status, or organizational unit.
 * @common_tasks
 *   - Finding a partner: Use the global search bar or entity-specific filters
 *   - Creating a partner: Click 'Create Partner' button (requires PARTNER_CREATE permission)
 *   - Editing a partner: Click on any partner row to open details, then click Edit
 *   - Filtering partners: Use the advanced search and filter options in the left panel
 *   - Exporting data: Use the Export button to download partner lists in Excel format
 *   - Importing partners: Use the Import button to bulk upload partner data
 * @tabs Details:/partnerships/partners/:id, Contacts:/partnerships/partners/:id/contacts, Interactions:/partnerships/partners/:id/interactions, Data:/partnerships/partners/:id/data
 */
@Component({
  selector: 'app-partner',
  // ... component config
})
export class PartnerComponent {
  // ... component implementation
}
```

### 2. Detail/View Pages

Components that show details for a specific entity.

```typescript
/**
 * @uiEntity Partner
 * @route /partnerships/partners/:id
 * @description View and edit detailed partner information including contact details, address, organizational data, and associated documents. Central place for managing all aspects of a partner organization.
 * @capabilities view_partner_details, edit_partner_info, upload_logo, manage_documents, view_contacts, create_interactions, edit_address, update_status
 * @synonyms organization_details, partner_profile, entity_view, collaborator_info
 * @mandatoryFields name, partnerType, status, partnerOfficeId
 * @help_when_stuck This page shows complete partner information. Click Edit to modify details, use tabs to navigate between sections, or click the logo area to upload a new partner logo. All fields are organized by category for easy access.
 * @common_tasks
 *   - Editing partner info: Click the Edit button and modify the form fields
 *   - Uploading logo: Click on the logo/image area to upload a new partner logo
 *   - Viewing contacts: Go to the Contacts tab to see people associated with this partner
 *   - Adding interactions: Go to Interactions tab and click 'Add Interaction'
 *   - Managing documents: Scroll down to the Documents section to upload or view files
 *   - Updating address: Edit the address fields in the Contact Information section
 * @tabs Details:/partnerships/partners/:id, Contacts:/partnerships/partners/:id/contacts, Interactions:/partnerships/partners/:id/interactions, Data:/partnerships/partners/:id/data
 */
@Component({
  selector: 'app-partner-view',
  // ... component config
})
export class PartnerViewComponent {
  // ... component implementation
}
```

### 3. Modal/Dialog Components

Components that appear in modals or dialogs.

```typescript
/**
 * @uiEntity Interaction
 * @route Modal dialog (no direct route)
 * @description Create and edit interaction records including meetings, calls, emails, and other communications with partners and contacts. Supports AI transcription and file attachments.
 * @capabilities create_interaction, edit_interaction, add_participants, upload_documents, ai_transcription, schedule_followup, set_interaction_type
 * @synonyms meeting, communication, event, activity, engagement, touchpoint
 * @mandatoryFields type, date, subject, contactId
 * @help_when_stuck Fill in the interaction type, date, and subject. Add participants using email addresses or selecting contacts. Use the AI transcription feature to quickly populate interaction details from audio or images.
 * @common_tasks
 *   - Recording a meeting: Select 'Meeting' type, add date/time, participants, and notes
 *   - Logging a phone call: Choose 'Phone Call' type, add contact, and conversation summary
 *   - Adding participants: Use email addresses or select from contact list
 *   - Using AI transcription: Click the transcribe button to process audio/image files
 *   - Attaching documents: Use the document section to upload relevant files
 *   - Setting follow-up: Add future interaction reminders or next steps
 */
@Component({
  selector: 'app-interaction-modal',
  // ... component config
})
export class InteractionModalComponent {
  // ... component implementation
}
```

### 4. Tab Components

Components that organize content into tabs.

```typescript
/**
 * @uiEntity Partner
 * @route /partnerships/partners/:id/*
 * @description Tab navigation for partner details, providing organized access to different aspects of partner information including basic details, contacts, interactions, and analytics.
 * @capabilities navigate_partner_sections, view_partner_details, access_related_data
 * @synonyms partner_navigation, partner_sections, partner_tabs
 * @help_when_stuck Use the tabs to navigate between different sections of partner information. Details shows basic info, Contacts shows people, Interactions shows communication history, and Data shows analytics.
 * @common_tasks
 *   - Viewing basic info: Stay on the Details tab
 *   - Finding contacts: Click the Contacts tab
 *   - Checking interaction history: Click the Interactions tab
 *   - Viewing analytics: Click the Data tab
 * @tabs Details:/partnerships/partners/:id, Contacts:/partnerships/partners/:id/contacts, Interactions:/partnerships/partners/:id/interactions, Data:/partnerships/partners/:id/data
 */
@Component({
  selector: 'app-partner-tabs',
  // ... component config
})
export class PartnerTabsComponent {
  // ... component implementation
}
```

## 🔧 Button Documentation (Optional)

For even richer context, you can document individual buttons and actions:

```typescript
export class PartnerViewComponent {
  
  /**
   * @uiButton edit_partner
   * @description Switches to edit mode for partner information
   * @label Edit Partner
   * @icon pi pi-pencil
   * @when_to_use When partner information needs updating, correcting details, adding new information
   * @permissions PARTNER_UPDATE
   */
  startEditing() {
    // implementation
  }

  /**
   * @uiButton save_partner
   * @description Saves changes and returns to view mode
   * @label Save Changes
   * @icon pi pi-check
   * @when_to_use After modifying partner information in edit mode
   * @permissions PARTNER_UPDATE
   */
  saveChanges() {
    // implementation
  }
}
```

## 🏃‍♂️ Running the Generator

### Command Line Options

```bash
# Basic usage
generate_frontend_tools.bat "../UNOPS.PAO.ClientApp"

# With custom output directory
generate_frontend_tools.bat "../UNOPS.PAO.ClientApp" "../UNOPS.PAO.AIService/config"

# PowerShell with environment
.\generate_frontend_tools.ps1 -AngularProject "../UNOPS.PAO.ClientApp" -OutputDir "../UNOPS.PAO.AIService/config"
```

### Python Direct Usage

```bash
cd ToolsJsonGenerator
python generate_frontend_tools.py --angular-project "../UNOPS.PAO.ClientApp" --output-dir "../UNOPS.PAO.AIService/config" --environment dev
```

## 📋 Documentation Checklist

For each major component, ensure you have documented:

### ✅ List Components
- [ ] Entity name matches backend
- [ ] Route is correct
- [ ] Capabilities include search, create, edit, delete, export
- [ ] Help explains how to find and create entities
- [ ] Common tasks cover search, create, edit workflows

### ✅ Detail Components  
- [ ] Entity name matches backend
- [ ] Route includes :id parameter
- [ ] Capabilities include view, edit, upload, manage sections
- [ ] Help explains edit mode and navigation
- [ ] Common tasks cover editing, uploading, managing related data
- [ ] Tabs are documented if present

### ✅ Modal Components
- [ ] Entity name matches backend
- [ ] Route indicates modal nature
- [ ] Capabilities include create/edit specific to modal
- [ ] Help explains form filling and special features
- [ ] Common tasks cover different types of interactions

### ✅ All Components
- [ ] Description is clear and business-focused
- [ ] Synonyms include terms users actually use
- [ ] Mandatory fields match backend requirements
- [ ] Help addresses common confusion points
- [ ] Common tasks are step-by-step instructions

## 🎯 Best Practices

### 1. User-Centric Language
✅ **Do**: "Create a new partner organization"  
❌ **Don't**: "Instantiate a Partner entity via POST API"

### 2. Specific, Actionable Help
✅ **Do**: "Click the + button in the top right to create new partners"  
❌ **Don't**: "Use the create functionality"

### 3. Address Real User Scenarios
✅ **Do**: "When you can't find a partner, try searching by organization name or location"  
❌ **Don't**: "Search functionality is available"

### 4. Include Permission Context
✅ **Do**: "Click 'Create Partner' button (requires PARTNER_CREATE permission)"  
❌ **Don't**: "Click 'Create Partner' button"

### 5. Troubleshooting Mindset
✅ **Do**: "If the Edit button is disabled, you may not have update permissions for this partner"  
❌ **Don't**: "Edit functionality exists"

## 🔗 Integration with AI Assistant

Once documented and generated, your AI assistant can:

### Provide Page-Specific Help
- **User**: "How do I use this page?"
- **AI**: *Looks up current route, provides specific guidance for that page*

### Explain Capabilities  
- **User**: "What can I do with partners?"
- **AI**: *Lists all partner capabilities from both UI and API perspectives*

### Guide Through Tasks
- **User**: "I need to create a new vendor"
- **AI**: *Uses UI guidance to walk through partner creation process*

### Troubleshoot Issues
- **User**: "I can't edit this partner"
- **AI**: *Checks permissions and provides specific help for partner editing*

## 🚀 What's Next

### Phase 1: Document Core Entities ✅
- Partner components (list, view, tabs)
- Contact components (list, view, tabs)  
- Interaction components (list, modal)

### Phase 2: Expand Coverage
- [ ] Admin components
- [ ] Search and filter components
- [ ] Dashboard and analytics components
- [ ] Settings and configuration components

### Phase 3: Advanced Features
- [ ] Workflow-specific guidance
- [ ] Role-based help customization
- [ ] Integration tutorials
- [ ] Keyboard shortcut documentation

---

**🎉 Result**: Your AI assistant becomes a contextual UI guide that can help users navigate and use every aspect of your Angular application! 