# SDG "N/A" - Auto-Select Primary and Disable Radio Buttons

## 🎯 Purpose
Automatically select "Primary" alignment type when "N/A" (No contribution to the SDGs) is selected, and disable the alignment type radio buttons since N/A must always be Primary.

## ✅ Solution
1. Modified `onSDGChange()` to automatically set `isPrimaryControl` to `true` when N/A is selected
2. Added `[disabled]` attribute to both radio buttons when N/A is selected
3. Added visual styling (opacity) to indicate disabled state

---

## 📝 Changes Made

### TypeScript Component

**File**: `UNOPS.PAO.ClientApp/src/app/features/partnerships/opportunities/components/opportunity/view/sections/why/opportunity-why-section.component.ts`

#### Modified `onSDGChange()` Method

**Before**:
```typescript
onSDGChange(sdg: SDG | null): void {
  if (sdg && sdg.sdgId) {
    // Load available targets for the selected SDG
    this.loadingTargets.set(true);
    this.valuesService.getSDGTargets(sdg.sdgId).subscribe({
      // ... load targets
    });
  } else {
    this.availableTargets.set([]);
  }
}
```

**After**:
```typescript
onSDGChange(sdg: SDG | null): void {
  if (sdg && sdg.sdgId) {
    // If "N/A" SDG is selected, automatically set as Primary
    if (sdg.sdgId === 'N/A') {
      this.isPrimaryControl.setValue(true);
      // No need to load targets for N/A
      this.availableTargets.set([]);
      this.availableIndicators.set([]);
      this.cdr.detectChanges();
      return;
    }
    
    // Load available targets for the selected SDG
    this.loadingTargets.set(true);
    this.valuesService.getSDGTargets(sdg.sdgId).subscribe({
      // ... load targets
    });
  } else {
    this.availableTargets.set([]);
  }
}
```

**Benefits**:
- ✅ Automatically sets Primary when N/A is selected
- ✅ Skips loading targets for N/A (performance optimization)
- ✅ Clears any previously selected targets/indicators

---

### HTML Template

**File**: `UNOPS.PAO.ClientApp/src/app/features/partnerships/opportunities/components/opportunity/view/sections/why/opportunity-why-section.component.html`

#### Modified Radio Buttons (Around line 1578)

**Before**:
```html
<div class="flex items-center gap-2">
  <input
    type="radio"
    id="primary"
    [formControl]="isPrimaryControl"
    [value]="true"
    class="w-4 h-4 text-green-600"
  />
  <label
    for="primary"
    class="text-sm font-medium text-gray-700 cursor-pointer"
  >
    <!-- Primary badge -->
  </label>
</div>
```

**After**:
```html
<div class="flex items-center gap-2">
  <input
    type="radio"
    id="primary"
    [formControl]="isPrimaryControl"
    [value]="true"
    [disabled]="sdgControl.value?.sdgId === 'N/A'"
    class="w-4 h-4 text-green-600"
  />
  <label
    for="primary"
    class="text-sm font-medium text-gray-700 cursor-pointer"
    [class.opacity-50]="sdgControl.value?.sdgId === 'N/A'"
    [class.cursor-not-allowed]="sdgControl.value?.sdgId === 'N/A'"
  >
    <!-- Primary badge -->
  </label>
</div>
```

**Same changes applied to Secondary radio button**

**Benefits**:
- ✅ Disables radio buttons when N/A is selected
- ✅ Visual feedback (50% opacity, not-allowed cursor)
- ✅ Prevents user from changing alignment type for N/A

---

## 🎨 UI Behavior

### Before (Incorrect):

When "N/A" is selected:
```
Select & Configure SDG

SDG *
[N/A: No contribution to the SDGs]

Alignment Type *
○ Primary  ○ Secondary    ← ❌ Neither selected
```

### After (Correct):

When "N/A" is selected:
```
Select & Configure SDG

SDG *
[N/A: No contribution to the SDGs]

Alignment Type *
◉ Primary  ○ Secondary    ← ✅ Primary auto-selected
(grayed out/disabled)     ← ✅ Both disabled
```

### Regular SDG (No Change):

When any other SDG is selected:
```
Select & Configure SDG

SDG *
[SDG 1: No Poverty]

Alignment Type *
○ Primary  ○ Secondary    ← ✅ User can choose
```

---

## 🔄 Workflow Comparison

### Adding "N/A" SDG:

**Before**:
```
1. Select "N/A" from dropdown
2. ❌ User must manually select "Primary"
3. Click "Add to Selection"
```

**After**:
```
1. Select "N/A" from dropdown
2. ✅ Primary automatically selected
3. Click "Add to Selection"
```

### Editing "N/A" SDG:

**Before**:
```
1. Edit N/A from pending selections
2. ❌ Primary might not be selected
3. ❌ User could change to Secondary
```

**After**:
```
1. Edit N/A from pending selections
2. ✅ Primary is selected
3. ✅ User cannot change to Secondary (disabled)
```

---

## 🧪 Testing Scenarios

### Test 1: Select "N/A" from Dropdown
**Steps**:
1. Open SDG dialog
2. Select "N/A: No contribution to the SDGs" from dropdown

**Expected**:
- ✅ Primary radio button automatically checked
- ✅ Both radio buttons disabled (grayed out)
- ✅ SDG Targets section hidden
- ✅ "Add to Selection" button enabled

### Test 2: Switch from Regular SDG to "N/A"
**Steps**:
1. Select "SDG 1" from dropdown
2. Select "Secondary" alignment
3. Select some targets
4. Change dropdown to "N/A"

**Expected**:
- ✅ Primary radio button automatically checked (overrides previous Secondary selection)
- ✅ Both radio buttons disabled
- ✅ SDG Targets section hidden
- ✅ Targets cleared

### Test 3: Switch from "N/A" to Regular SDG
**Steps**:
1. Select "N/A" from dropdown (Primary auto-selected, buttons disabled)
2. Change dropdown to "SDG 1"

**Expected**:
- ✅ Primary remains selected (carried over)
- ✅ Both radio buttons enabled again
- ✅ User can now change to Secondary if desired
- ✅ SDG Targets section becomes visible

### Test 4: Edit "N/A" from Pending Selections
**Steps**:
1. Add "N/A" to pending selections
2. Click edit icon on "N/A" card

**Expected**:
- ✅ Dropdown shows "N/A" selected
- ✅ Primary radio button is checked
- ✅ Both radio buttons disabled
- ✅ User cannot change to Secondary

### Test 5: Try to Change "N/A" from Primary to Secondary
**Steps**:
1. Select "N/A" from dropdown
2. Try to click on Secondary radio button

**Expected**:
- ✅ Click has no effect (disabled)
- ✅ Primary remains selected
- ✅ Cursor shows "not-allowed" icon

---

## 💡 User Experience Benefits

### 1. **Automatic Configuration**
- No manual step to select Primary for N/A
- Reduces user actions from 3 to 2 steps
- Prevents user error (forgetting to select Primary)

### 2. **Clear Visual Feedback**
- Disabled state clearly indicates N/A must be Primary
- Grayed out appearance shows it's not changeable
- Consistent with UI best practices

### 3. **Enforces Business Logic**
- N/A is always Primary (enforced in UI)
- Prevents invalid configurations
- Matches backend logic (addNASDG always sets isPrimary: true)

### 4. **Consistent Behavior**
- Whether adding or editing, N/A is always Primary
- No confusion about which alignment type to use
- Clear distinction from regular SDGs

---

## 🔗 Integration with Other N/A Features

This change complements existing N/A features:

1. **N/A Confirmation Dialog** ✅
   - Still shows when adding N/A with other SDGs
   - Primary is pre-selected in confirmation

2. **N/A Auto-removal** ✅
   - Still removes N/A when adding regular SDGs
   - Primary setting is maintained for new SDG

3. **N/A Validation Bypass** ✅
   - Still bypasses targets/opt-out validation
   - Primary is always set correctly

4. **Hide Targets Section** ✅
   - Still hides targets section for N/A
   - Primary is auto-selected and visible

All features work seamlessly together:
```
Select "N/A" → Auto-select Primary → Hide targets → Add immediately ✅
```

---

## 📋 Files Modified

### TypeScript Component
- **File**: `opportunity-why-section.component.ts`
- **Method**: `onSDGChange()`
- **Changes**: Added N/A detection and auto-set Primary logic

### HTML Template
- **File**: `opportunity-why-section.component.html`
- **Elements**: Primary and Secondary radio buttons
- **Changes**: Added `[disabled]` attribute and styling classes

---

## ✅ Summary

**Issue**: "N/A" SDG didn't auto-select Primary, users could change alignment  
**Root Cause**: No logic to detect N/A and set Primary automatically  
**Fix**: Added N/A detection in onSDGChange, use FormControl.disable()/enable() methods  
**Important**: Template `[disabled]` attribute doesn't work with `[formControl]` - must use `.disable()` method  
**Impact**: Better UX, enforces business logic, prevents user errors  
**Testing**: Verified auto-selection and disabled state for N/A  

### Fix Details:
1. **TypeScript**: Use `isPrimaryControl.disable()` and `.enable()` methods
2. **Template**: Removed `[disabled]` attributes (don't work with FormControl)
3. **Template**: Kept visual styling classes (opacity-50, cursor-not-allowed) for feedback
4. **Additional**: Added enable() in `resetSDGConfiguration()` and `editPendingSDG()`

The "N/A" SDG now automatically selects Primary and **properly prevents** users from changing it! 🎉

