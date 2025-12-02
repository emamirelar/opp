# SDG Opt-Out Display Fix

## 🐛 Issue
The "I want to opt out" indicator was not displaying in read-only mode on the main opportunity page after saving SDGs with the opt-out option checked. It was only showing in the dialog during editing.

## ✅ Solution
Added logic to display the opt-out indicator in read-only mode (main opportunity view) when an SDG has `skipTargetsAndIndicators` set to `true`.

---

## 📝 Changes Made

### HTML Template Update

**File**: `UNOPS.PAO.ClientApp/src/app/features/partnerships/opportunities/components/opportunity/view/sections/why/opportunity-why-section.component.html`

**Location**: Read-only SDG display section (around line 200)

#### Before (Missing Opt-Out Display):
```html
<!-- Display Targets and Indicators -->
@if (sdg.targets && sdg.targets.length > 0) {
  <div class="mt-3 pl-4 border-l-2 border-unops-primary-200 space-y-2">
    <!-- Targets and indicators display -->
  </div>
}
```

**Problem**: Only showed targets, no logic to display opt-out status.

#### After (With Opt-Out Display):
```html
<!-- Display Targets and Indicators or Opt-Out Status -->
@if (sdg.skipTargetsAndIndicators) {
  <div class="mt-3">
    <div class="flex items-start gap-2 bg-amber-50 border border-amber-200 rounded-lg p-3">
      <i class="pi pi-info-circle text-amber-600 text-sm mt-0.5"></i>
      <div class="text-sm text-amber-800">
        <span class="font-medium">{{ "label.opportunity.skipTargetsAndIndicators" | translate }}</span>
        <p class="text-xs text-amber-700 mt-1">
          {{ "message.opportunity.skipTargetsAndIndicatorsInfo" | translate }}
        </p>
      </div>
    </div>
  </div>
} @else if (sdg.targets && sdg.targets.length > 0) {
  <div class="mt-3 pl-4 border-l-2 border-unops-primary-200 space-y-2">
    <!-- Targets and indicators display -->
  </div>
}
```

**Benefits**:
- ✅ Checks `skipTargetsAndIndicators` first (priority)
- ✅ Shows amber info box when opt-out is selected
- ✅ Displays explanatory text
- ✅ Falls back to showing targets if not opted out

---

## 🎨 Visual Design

### Opt-Out Indicator Styling:
- **Background**: Amber (amber-50) with amber border
- **Icon**: Information circle (pi-info-circle) in amber-600
- **Text Color**: Amber-800 for title, amber-700 for description
- **Layout**: Flexbox with icon and text side-by-side
- **Spacing**: Proper padding and gaps for readability

### Consistency:
- Matches the opt-out checkbox styling in the dialog (amber theme)
- Uses the same translation keys as the dialog
- Provides clear visual distinction from targets display

---

## 📊 Display Logic

### Priority Order:
1. **First Check**: Is `skipTargetsAndIndicators` true?
   - YES → Show amber opt-out indicator
   - NO → Continue to next check

2. **Second Check**: Are there targets selected?
   - YES → Show targets and indicators
   - NO → Show nothing (empty state)

### Truth Table:

| skipTargetsAndIndicators | targets.length | Display Result |
|--------------------------|----------------|----------------|
| `true`                   | 0              | ✅ Opt-out indicator |
| `true`                   | > 0            | ✅ Opt-out indicator (opt-out takes priority) |
| `false` / `null`         | > 0            | ✅ Targets and indicators |
| `false` / `null`         | 0              | ❌ Nothing (empty) |

**Note**: Opt-out always takes precedence over targets display.

---

## 🌍 Translation Keys Used

Both translation keys already existed in all 4 language files:

### English (`en.json`):
```json
{
  "label.opportunity.skipTargetsAndIndicators": "I want to opt out as this will be identified if development is pursued.",
  "message.opportunity.skipTargetsAndIndicatorsInfo": "Selecting this option will skip target and indicator selection for this SDG."
}
```

### Spanish (`span.json`):
```json
{
  "label.opportunity.skipTargetsAndIndicators": "Deseo optar por no participar ya que esto se identificará si se continúa el desarrollo.",
  "message.opportunity.skipTargetsAndIndicatorsInfo": "Seleccionar esta opción omitirá la selección de metas e indicadores para este ODS."
}
```

### French (`fr.json`):
```json
{
  "label.opportunity.skipTargetsAndIndicators": "Je souhaite me désengager car cela sera identifié si le développement est poursuivi.",
  "message.opportunity.skipTargetsAndIndicatorsInfo": "La sélection de cette option ignorera la sélection de cibles et d'indicateurs pour cet ODD."
}
```

### Portuguese (`pt.json`):
```json
{
  "label.opportunity.skipTargetsAndIndicators": "Desejo não participar, pois isso será identificado se o desenvolvimento prosseguir.",
  "message.opportunity.skipTargetsAndIndicatorsInfo": "Selecionar esta opção ignorará a seleção de metas e indicadores para este ODS."
}
```

---

## 🧪 Testing

### Test Scenario 1: SDG with Opt-Out
**Steps**:
1. Open SDG dialog
2. Select an SDG
3. Check "I want to opt out" checkbox
4. Click "Add to Selection"
5. Click "Add 1 SDG"

**Expected Result**:
- ✅ SDG card displays in read-only mode
- ✅ Shows amber opt-out indicator box
- ✅ Displays opt-out label and info text
- ✅ No targets or indicators shown

### Test Scenario 2: SDG with Targets (No Opt-Out)
**Steps**:
1. Open SDG dialog
2. Select an SDG
3. Select targets and indicators
4. Don't check opt-out
5. Click "Add to Selection"
6. Click "Add 1 SDG"

**Expected Result**:
- ✅ SDG card displays in read-only mode
- ✅ Shows targets with indicators
- ✅ Blue border on left side
- ✅ No opt-out indicator shown

### Test Scenario 3: Mixed SDGs
**Steps**:
1. Add SDG 1 with targets (no opt-out)
2. Add SDG 4 with opt-out checked
3. Add SDG 6 with targets (no opt-out)
4. Click "Add 3 SDGs"

**Expected Result**:
- ✅ SDG 1: Shows targets and indicators
- ✅ SDG 4: Shows amber opt-out indicator
- ✅ SDG 6: Shows targets and indicators

### Test Scenario 4: Edit Existing SDG
**Steps**:
1. Open opportunity with saved SDGs
2. Verify opt-out indicators display correctly
3. Click "Edit" on section
4. Verify indicators still show correctly in edit mode

**Expected Result**:
- ✅ Opt-out indicators persist correctly
- ✅ Display remains consistent in read/edit modes

---

## 🔄 Consistency Check

### Dialog View (Edit Mode):
```html
@if (sdg.skipTargetsAndIndicators) {
  <div class="text-xs text-gray-500 italic">
    ({{ "label.opportunity.skipTargetsAndIndicators" | translate }})
  </div>
}
```
- Shows as italic gray text in parentheses
- Condensed display for card view

### Read-Only View (Main Page):
```html
@if (sdg.skipTargetsAndIndicators) {
  <div class="flex items-start gap-2 bg-amber-50 border border-amber-200 rounded-lg p-3">
    <i class="pi pi-info-circle text-amber-600 text-sm mt-0.5"></i>
    <div class="text-sm text-amber-800">
      <span class="font-medium">{{ "label.opportunity.skipTargetsAndIndicators" | translate }}</span>
      <p class="text-xs text-amber-700 mt-1">
        {{ "message.opportunity.skipTargetsAndIndicatorsInfo" | translate }}
      </p>
    </div>
  </div>
}
```
- Shows as prominent amber info box
- More detailed with explanation
- Better visibility for read mode

**Both displays**:
- ✅ Use same translation keys
- ✅ Properly indicate opt-out status
- ✅ Appropriate styling for context

---

## 📋 Files Modified

### HTML Template
- **File**: `UNOPS.PAO.ClientApp/src/app/features/partnerships/opportunities/components/opportunity/view/sections/why/opportunity-why-section.component.html`
- **Lines Modified**: ~200-248 (read-only SDG display section)
- **Change Type**: Added opt-out indicator display logic

### No New Translation Keys Needed
- All required translation keys already existed
- No changes to language files required

---

## ✅ Summary

**Issue**: "I want to opt out" indicator missing in read-only view  
**Root Cause**: Missing conditional check for `skipTargetsAndIndicators` in read-only display  
**Fix**: Added priority check for opt-out status before showing targets  
**Impact**: Opt-out selection now properly displays on main opportunity page  
**Testing**: Verified in all display modes and scenarios  

The fix ensures that users can clearly see when an SDG has been configured with the opt-out option, maintaining data transparency and improving user understanding of SDG configurations.

