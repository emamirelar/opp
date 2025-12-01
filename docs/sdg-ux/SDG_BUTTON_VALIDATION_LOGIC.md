# SDG Button Validation Logic

## 🎯 Purpose
The "Add SDG to Selection" button should only be enabled when the user has completed the configuration. This prevents adding incomplete or invalid SDG selections.

---

## ✅ Button Enabled When:

### Scenario 1: Skip Targets Checked
```
✓ SDG Selected: "SDG 1: No Poverty"
✓ Alignment: Primary/Secondary selected
✓ Skip targets: [✓] Checked

→ Button ENABLED ✅
```
**Reason**: User explicitly chose to skip targets/indicators, so configuration is complete.

---

### Scenario 2: At Least One Target Selected
```
✓ SDG Selected: "SDG 1: No Poverty"
✓ Alignment: Primary/Secondary selected
✓ Skip targets: [ ] Not checked
✓ Targets: [✓] At least one target selected

→ Button ENABLED ✅
```
**Reason**: User selected specific target(s), configuration is meaningful.

---

## ❌ Button Disabled When:

### Scenario 1: No SDG Selected
```
✗ SDG Selected: (none)

→ Button DISABLED ❌
```
**Reason**: Cannot add an SDG without selecting one.

---

### Scenario 2: SDG Selected but No Targets and Skip Not Checked
```
✓ SDG Selected: "SDG 1: No Poverty"
✓ Alignment: Primary/Secondary selected
✗ Skip targets: [ ] Not checked
✗ Targets: [ ] No targets selected

→ Button DISABLED ❌
```
**Reason**: Configuration is incomplete - user must either select targets or check skip option.

---

## 🔧 Technical Implementation

### Computed Property (TypeScript)

```typescript
readonly isSDGConfigurationComplete = computed(() => {
  // Must have an SDG selected
  if (!this.sdgControl.value) {
    return false;
  }
  
  // If skip targets is checked, configuration is complete
  if (this.skipTargetsControl.value) {
    return true;
  }
  
  // Otherwise, at least one target must be selected
  const selectedTargetsMap = this.selectedTargets();
  return selectedTargetsMap.size > 0;
});
```

### Logic Flow

```
START
  │
  ├─> Is SDG selected?
  │   ├─ NO  → Return FALSE (disabled)
  │   └─ YES → Continue
  │
  ├─> Is "Skip targets" checked?
  │   ├─ YES → Return TRUE (enabled)
  │   └─ NO  → Continue
  │
  └─> Are any targets selected?
      ├─ YES → Return TRUE (enabled)
      └─ NO  → Return FALSE (disabled)
```

### HTML Template Binding

```html
<p-button
  [label]="'button.addSDGToSelection' | translate"
  icon="pi pi-plus"
  severity="primary"
  size="small"
  (onClick)="addSDGToPendingSelection()"
  [disabled]="!isSDGConfigurationComplete()"
/>
```

---

## 📊 State Transition Table

| SDG Selected | Skip Checked | Targets Selected | Button State |
|--------------|--------------|------------------|--------------|
| ❌ No        | -            | -                | 🔴 Disabled  |
| ✅ Yes       | ✅ Yes       | -                | 🟢 Enabled   |
| ✅ Yes       | ❌ No        | ✅ Yes (≥1)      | 🟢 Enabled   |
| ✅ Yes       | ❌ No        | ❌ No (0)        | 🔴 Disabled  |

---

## 🎨 Visual Feedback

### Button States

**Disabled State:**
```
[Add SDG to Selection →] (grayed out, no hover effect)
```
- Gray background
- No hover animation
- Cursor: not-allowed
- User cannot click

**Enabled State:**
```
[Add SDG to Selection →] (blue, clickable)
```
- Primary blue background
- Hover effect (darker blue)
- Cursor: pointer
- Click triggers action

---

## 🧪 Test Scenarios

### Test 1: Initial State
**Steps:**
1. Open SDG dialog
2. Select an SDG from dropdown
3. Don't check skip checkbox
4. Don't select any targets

**Expected**: Button is DISABLED ❌

---

### Test 2: Skip Targets Flow
**Steps:**
1. Open SDG dialog
2. Select an SDG from dropdown
3. Check "Skip targets and indicators"

**Expected**: Button is ENABLED ✅

---

### Test 3: Select Target Flow
**Steps:**
1. Open SDG dialog
2. Select an SDG from dropdown
3. Leave skip checkbox unchecked
4. Select at least one target

**Expected**: Button is ENABLED ✅

---

### Test 4: Uncheck Skip After Checking
**Steps:**
1. Open SDG dialog
2. Select an SDG from dropdown
3. Check "Skip targets and indicators" (button enables)
4. Uncheck "Skip targets and indicators"
5. Don't select any targets

**Expected**: Button is DISABLED ❌

---

### Test 5: Select Then Deselect All Targets
**Steps:**
1. Open SDG dialog
2. Select an SDG from dropdown
3. Select a target (button enables)
4. Deselect the target

**Expected**: Button is DISABLED ❌

---

## 💡 User Experience Benefits

### 1. **Prevents Invalid Data**
- ✅ Cannot add SDG without meaningful configuration
- ✅ Forces user to make a choice (skip or select targets)
- ✅ Reduces data quality issues

### 2. **Clear Feedback**
- ✅ Disabled button signals incomplete configuration
- ✅ Button enables when configuration is valid
- ✅ Visual cue guides user to next step

### 3. **Guided Workflow**
- ✅ User knows what to do (select targets or skip)
- ✅ No ambiguity about when they can proceed
- ✅ Reduces user errors and confusion

### 4. **Consistent Behavior**
- ✅ Same logic for both "Add" and "Update" modes
- ✅ Predictable behavior across all scenarios
- ✅ Matches validation patterns in rest of app

---

## 🔄 Integration with Existing Validation

### When Button Clicked (Even if Enabled)

The existing validation in `addSDGToPendingSelection()` still runs:

```typescript
addSDGToPendingSelection(): void {
  const sdg = this.sdgControl.value;
  
  // Validation
  if (!sdg) {
    this.showValidationError.set(true);
    return;
  }
  
  // Check for duplicates
  if (currentPending.some(s => s.sdgId === sdg.sdgId)) {
    this.feedbackService.showErrorToast({
      detail: 'This SDG is already in your selection'
    });
    return;
  }
  
  // ... rest of logic
}
```

**Two-Layer Validation:**
1. **Button Disabled**: Prevents click for incomplete configuration
2. **Method Validation**: Handles edge cases and duplicate detection

---

## ✅ Implementation Status

- ✅ Computed property created: `isSDGConfigurationComplete()`
- ✅ HTML button binding updated
- ✅ Both "Add" and "Update" buttons use validation
- ✅ Build successful (no compilation errors)
- ✅ Logic tested with multiple scenarios
- ✅ Consistent with UX best practices

---

## 📝 Summary

**Before**: Button only disabled when no SDG selected

**After**: Button disabled when:
- No SDG selected, OR
- SDG selected but no targets selected AND skip not checked

**Result**: More intuitive UX with clear guidance on what's required to proceed! 🎉

