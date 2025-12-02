# SDG "N/A" - Hide Targets Section

## 🎯 Purpose
Hide the "SDG Targets" section when the "N/A" (No contribution to the SDGs) option is selected, since it doesn't require targets or indicators.

## ✅ Solution
Modified the template condition to check if the selected SDG is not "N/A" before displaying the targets section.

---

## 📝 Changes Made

### HTML Template Update

**File**: `UNOPS.PAO.ClientApp/src/app/features/partnerships/opportunities/components/opportunity/view/sections/why/opportunity-why-section.component.html`

**Location**: Around line 1662

#### Before:
```html
<!-- SDG Targets Section -->
@if (sdgControl.value) {
  <div class="border-t border-gray-200 pt-4 mt-4">
    <label class="block text-sm font-semibold text-gray-700 mb-3">
      {{ "label.opportunity.sdgTargets" | translate }}
    </label>
    <!-- ... targets and indicators UI ... -->
  </div>
}
```

**Problem**: Showed the targets section for all SDGs, including "N/A".

#### After:
```html
<!-- SDG Targets Section (Hidden for N/A SDG) -->
@if (sdgControl.value && sdgControl.value.sdgId !== 'N/A') {
  <div class="border-t border-gray-200 pt-4 mt-4">
    <label class="block text-sm font-semibold text-gray-700 mb-3">
      {{ "label.opportunity.sdgTargets" | translate }}
    </label>
    <!-- ... targets and indicators UI ... -->
  </div>
}
```

**Solution**: Added condition `&& sdgControl.value.sdgId !== 'N/A'` to hide the entire targets section when "N/A" is selected.

---

## 🎨 UI Behavior

### Before (Incorrect):
When "N/A" SDG is selected:
```
Select & Configure SDG
━━━━━━━━━━━━━━━━━━━━━━━━━━━━

SDG *
[N/A: No contribution to the SDGs]

Alignment Type *
◉ Primary  ○ Secondary

SDG Targets                          ← ❌ Shown (unnecessary)
━━━━━━━━━━━━━━━━━━━━━━━━━━━━
ℹ Select specific targets and...
┌─────────────────────────────────┐
│ ☐ I want to opt out...          │
└─────────────────────────────────┘
```

### After (Correct):
When "N/A" SDG is selected:
```
Select & Configure SDG
━━━━━━━━━━━━━━━━━━━━━━━━━━━━

SDG *
[N/A: No contribution to the SDGs]

Alignment Type *
◉ Primary  ○ Secondary

                                    ← ✅ Hidden (correct)

[+ Add SDG to Selection]
```

### Regular SDG (No Change):
When any other SDG is selected:
```
Select & Configure SDG
━━━━━━━━━━━━━━━━━━━━━━━━━━━━

SDG *
[SDG 1: No Poverty]

Alignment Type *
◉ Primary  ○ Secondary

SDG Targets                          ← ✅ Still shows (correct)
━━━━━━━━━━━━━━━━━━━━━━━━━━━━
ℹ Select specific targets and...
```

---

## 🔄 Integration with Existing Logic

### Related N/A SDG Features:

1. **Add to Selection**: "N/A" can be added without targets
2. **Validation**: "N/A" bypasses targets/opt-out validation
3. **Update Configuration**: "N/A" editing doesn't require targets
4. **Button State**: "Add SDG to Selection" is enabled for "N/A" without targets

### Targets Section Visibility Rules:

| SDG Selected | sdgId     | Targets Section Visible? |
|--------------|-----------|--------------------------|
| None         | null      | ❌ Hidden                |
| N/A          | 'N/A'     | ❌ Hidden                |
| SDG 1        | '1'       | ✅ Visible               |
| SDG 4        | '4'       | ✅ Visible               |
| Any other    | any       | ✅ Visible               |

**Logic**:
```typescript
// Condition: sdgControl.value && sdgControl.value.sdgId !== 'N/A'
//
// If no SDG selected → false (hidden)
// If N/A selected → false (hidden)
// If any other SDG selected → true (visible)
```

---

## 🧪 Testing Scenarios

### Test 1: Select "N/A" SDG
**Steps**:
1. Open SDG dialog
2. Select "N/A: No contribution to the SDGs" from dropdown
3. Observe the form

**Expected**:
- ✅ SDG dropdown shows "N/A" selected
- ✅ Alignment Type section visible (Primary/Secondary)
- ✅ "SDG Targets" section is hidden
- ✅ "Add SDG to Selection" button is visible and enabled
- ✅ No opt-out checkbox visible
- ✅ No targets/indicators list visible

### Test 2: Select Regular SDG After "N/A"
**Steps**:
1. Select "N/A" from dropdown (targets section hidden)
2. Change dropdown to "SDG 1: No Poverty"
3. Observe the form

**Expected**:
- ✅ "SDG Targets" section becomes visible
- ✅ Shows opt-out checkbox
- ✅ Shows targets list (after loading)
- ✅ Button enabled (per previous feature)

### Test 3: Switch from Regular SDG to "N/A"
**Steps**:
1. Select "SDG 1" from dropdown (targets section visible)
2. Select some targets
3. Change dropdown to "N/A"
4. Observe the form

**Expected**:
- ✅ "SDG Targets" section becomes hidden
- ✅ Selected targets cleared/ignored
- ✅ Button remains enabled
- ✅ Can add "N/A" immediately

### Test 4: Editing "N/A" SDG from Pending
**Steps**:
1. Add "N/A" to pending selections
2. Click edit icon on "N/A" card
3. Observe the form

**Expected**:
- ✅ Dropdown shows "N/A" selected
- ✅ Primary alignment selected
- ✅ "SDG Targets" section hidden
- ✅ Can update or change to another SDG

---

## 💡 User Experience Benefits

### 1. **Cleaner Interface**
- Removes unnecessary UI elements for "N/A" SDG
- Less visual clutter
- Focuses user attention on relevant fields

### 2. **Prevents Confusion**
- Users won't wonder "Should I select targets for N/A?"
- Clear that N/A doesn't require configuration
- Aligns with the meaning of "No contribution to SDGs"

### 3. **Faster Workflow**
- Users can immediately click "Add to Selection" for N/A
- No need to scroll past targets section
- Reduced form height when N/A is selected

### 4. **Consistent Logic**
- UI matches the backend logic (N/A doesn't save targets)
- Validation logic matches UI (N/A bypasses target validation)
- Clear distinction between N/A and regular SDGs

---

## 📋 Files Modified

### HTML Template
- **File**: `UNOPS.PAO.ClientApp/src/app/features/partnerships/opportunities/components/opportunity/view/sections/why/opportunity-why-section.component.html`
- **Line**: ~1662
- **Change**: Updated condition from `@if (sdgControl.value)` to `@if (sdgControl.value && sdgControl.value.sdgId !== 'N/A')`

### No TypeScript Changes Required
- Existing logic already handles N/A properly
- Validation already bypasses N/A
- Button state logic already allows N/A without targets

---

## 🔗 Related Features

This change complements the existing N/A SDG features:

1. **N/A Confirmation Dialog** - Shows warning when adding N/A with other SDGs
2. **N/A Auto-removal** - Removes N/A when adding regular SDGs
3. **N/A Validation Bypass** - N/A doesn't require targets/opt-out
4. **Button Enable Logic** - Button enabled for N/A without targets

All features now work seamlessly together:
- Select "N/A" → No targets section → Button enabled → Add immediately ✅
- Select regular SDG → Targets section visible → Select targets or opt-out → Add ✅

---

## ✅ Summary

**Issue**: SDG Targets section shown for "N/A" SDG (unnecessary)  
**Root Cause**: Condition only checked if SDG was selected, not if it was "N/A"  
**Fix**: Added `&& sdgControl.value.sdgId !== 'N/A'` condition  
**Impact**: Cleaner UI, prevents confusion, matches backend logic  
**Testing**: Verified hiding/showing based on SDG selection  

The "N/A" SDG now provides a streamlined experience with no irrelevant configuration options! 🎉

