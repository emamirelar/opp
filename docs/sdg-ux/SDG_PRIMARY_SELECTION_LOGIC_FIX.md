# SDG Primary Selection Logic Fix

## 🐛 Issue
When switching from "N/A" SDG to a regular SDG, the Primary alignment type remained selected even when there was already a Primary SDG in the pending selections. This created a conflict and could result in multiple Primary SDGs.

### Reproduction:
1. Add SDG 1 as Primary
2. Select "N/A" from dropdown (auto-selects Primary, disables radio)
3. Change dropdown to SDG 4 (radio re-enables)
4. Primary still selected ❌ (should be Secondary since SDG 1 is already Primary)

## ✅ Solution
Modified `onSDGChange()` to intelligently reset the alignment type based on existing Primary SDGs when switching from "N/A" to a regular SDG.

---

## 📝 Logic Flow

### When Switching from "N/A" to Regular SDG:

```
User changes from N/A to SDG 4
        │
        ├─> Re-enable isPrimaryControl
        │
        ├─> Check: Is Primary currently selected? (from N/A)
        │   └─ YES → Continue checking
        │
        ├─> Check: Is there a Primary SDG in pending?
        │   ├─ NO → Keep Primary selected ✅
        │   └─ YES → Check if it's N/A
        │       ├─ YES (Primary is N/A) → Keep Primary (N/A will be removed) ✅
        │       └─ NO (Primary is another SDG) → Set to Secondary ✅
        │
        └─> Continue with loading targets
```

### Truth Table:

| Current Value | Has Primary in Pending? | Primary is N/A? | Result |
|---------------|-------------------------|-----------------|---------|
| Primary       | No                      | -               | Keep Primary ✅ |
| Primary       | Yes                     | Yes             | Keep Primary ✅ (N/A will be removed) |
| Primary       | Yes                     | No              | Set to Secondary ✅ |
| Secondary     | -                       | -               | Keep Secondary ✅ |

---

## 🔧 Technical Implementation

### Modified `onSDGChange()` Method

**Added Smart Primary Detection:**

```typescript
onSDGChange(sdg: SDG | null): void {
  if (sdg && sdg.sdgId) {
    if (sdg.sdgId === 'N/A') {
      // Handle N/A selection
      this.isPrimaryControl.setValue(true);
      this.isPrimaryControl.disable();
      return;
    } else {
      // For regular SDGs, ensure the control is enabled
      this.isPrimaryControl.enable();
      
      // Check if there's already a Primary SDG in pending selections
      const hasPrimary = this.hasPrimaryInPending();
      
      // If switching from N/A (which was Primary) to a regular SDG,
      // and there's already a Primary in pending, set to Secondary
      if (this.isPrimaryControl.value === true && hasPrimary) {
        // Check if the current Primary is N/A (which will be removed)
        const currentPending = this.pendingSDGSelections();
        const primarySDG = currentPending.find(s => s.isPrimary);
        
        // If the current Primary is NOT N/A, then we need to set this new SDG to Secondary
        if (primarySDG && primarySDG.sdgId !== 'N/A') {
          this.isPrimaryControl.setValue(false);
        }
      }
    }
    
    // ... load targets
  }
}
```

**Key Logic**:
1. Enable the control for regular SDGs
2. Check if control value is Primary (carried over from N/A)
3. Check if there's already a Primary in pending
4. Check if that Primary is N/A (which will be auto-removed)
5. Only set to Secondary if there's a non-N/A Primary

---

## 📊 Scenarios

### Scenario 1: N/A → Regular SDG (No Other Primary)
```
Pending: []
Current: N/A selected (Primary)
Action: Switch to SDG 4

Logic:
- Re-enable control ✓
- hasPrimaryInPending() = false
- Keep Primary = true ✅

Result: SDG 4 set as Primary ✅
```

### Scenario 2: N/A → Regular SDG (Primary is N/A)
```
Pending: [N/A (Primary)]
Current: N/A selected
Action: Switch to SDG 4

Logic:
- Re-enable control ✓
- hasPrimaryInPending() = true
- primarySDG.sdgId = 'N/A'
- Keep Primary = true ✅
(N/A will be auto-removed when adding SDG 4)

Result: SDG 4 set as Primary ✅
```

### Scenario 3: N/A → Regular SDG (Another Primary Exists) - THE FIX
```
Pending: [SDG 1 (Primary), N/A (Secondary - hypothetical)]
Current: N/A selected (Primary due to auto-select)
Action: Switch to SDG 4

Logic:
- Re-enable control ✓
- isPrimaryControl.value = true
- hasPrimaryInPending() = true
- primarySDG.sdgId = '1' (NOT 'N/A')
- Set Primary = false ✅

Result: SDG 4 set as Secondary ✅
```

### Scenario 4: Regular SDG → Another Regular SDG (Normal Flow)
```
Pending: [SDG 1 (Primary)]
Current: SDG 4 selected
Action: Switch to SDG 6

Logic:
- Control is already enabled
- isPrimaryControl.value = false (already Secondary)
- No changes needed

Result: SDG 6 keeps Secondary ✅
```

---

## 🧪 Testing Scenarios

### Test 1: N/A → Regular SDG with Existing Primary (THE FIX)
**Steps**:
1. Add SDG 1 as Primary to pending
2. Select "N/A" from dropdown (Primary auto-selected)
3. Change dropdown to SDG 4

**Expected**:
- ✅ Radio buttons re-enabled
- ✅ Secondary automatically selected (not Primary)
- ✅ Info message: "You can only have one Primary SDG..."
- ✅ Can manually change to Primary if desired

### Test 2: N/A → Regular SDG without Existing Primary
**Steps**:
1. Start with empty pending selections
2. Select "N/A" from dropdown (Primary auto-selected)
3. Change dropdown to SDG 4

**Expected**:
- ✅ Radio buttons re-enabled
- ✅ Primary remains selected
- ✅ No conflict, SDG 4 can be Primary

### Test 3: N/A → Regular SDG when N/A is Only Primary
**Steps**:
1. Add N/A to pending (as Primary)
2. Click edit on N/A
3. Change dropdown to SDG 4

**Expected**:
- ✅ Radio buttons re-enabled
- ✅ Primary remains selected
- ✅ N/A will be removed/replaced, so SDG 4 can be Primary

### Test 4: Regular SDG → Another Regular SDG (No N/A Involved)
**Steps**:
1. Add SDG 1 as Primary to pending
2. Select SDG 4 from dropdown (starts as Secondary)
3. Change dropdown to SDG 6

**Expected**:
- ✅ Radio buttons stay enabled
- ✅ Secondary remains selected
- ✅ No changes to alignment type
- ✅ Normal behavior (not affected by N/A logic)

---

## 💡 Benefits

### 1. **Prevents Primary Conflicts**
- Can't accidentally create two Primary SDGs
- Logic prevents conflicts when switching from N/A
- Maintains single Primary rule

### 2. **Smart Defaults**
- Automatically selects appropriate alignment based on context
- Considers existing Primary SDGs
- Handles N/A removal scenario correctly

### 3. **User Control Maintained**
- User can still manually change to Primary if needed
- Logic just provides smart defaults
- Doesn't prevent valid configurations

### 4. **Consistent Behavior**
- Works the same whether switching from N/A or not
- Respects existing Primary SDGs
- No confusion about which SDG is Primary

---

## 🔗 Integration with N/A Auto-Removal

This fix works seamlessly with the N/A auto-removal feature:

```
Scenario: [SDG 1 Primary, N/A Secondary] → Switch N/A to SDG 4
        │
        ├─> User edits N/A (was manually set as Secondary)
        │   └─> N/A loads, radio enabled, Secondary selected
        │
        ├─> User changes to SDG 4
        │   ├─> Radio stays enabled
        │   ├─> hasPrimaryInPending() = true (SDG 1)
        │   ├─> Primary is SDG 1 (not N/A)
        │   └─> Keep Secondary ✅
        │
        └─> User clicks "Update"
            └─> N/A replaced with SDG 4 (Secondary) ✅
```

---

## 📋 Files Modified

### TypeScript Component
- **File**: `opportunity-why-section.component.ts`
- **Method**: `onSDGChange()`
- **Change**: Added smart Primary detection logic when switching from N/A to regular SDG

### No HTML Changes
- Template already handles disabled state visually
- No changes needed

---

## ✅ Summary

**Issue**: Primary stayed selected when switching from N/A to regular SDG with existing Primary  
**Root Cause**: Control value not reset when re-enabling after N/A  
**Fix**: Added logic to check for existing Primary and set to Secondary if needed  
**Impact**: Prevents Primary conflicts, smart defaults, maintains single Primary rule  
**Testing**: Verified all scenarios with various Primary configurations  

The alignment type now intelligently adjusts when switching from "N/A" to regular SDGs! 🎉

