# SDG Validation Enhancement - Implementation Summary

## 🎯 Purpose
Ensure that each SDG in the "Add SDG" dialog has either at least one target selected OR the "I want to opt out" checkbox is checked before allowing the user to commit the changes.

## ✨ Features Implemented

### 1. **Validation at Commit Time**
When the user clicks the "Add x SDGs" button:
- All pending SDGs are validated
- SDGs that don't meet the criteria are flagged with validation errors
- User receives a toast notification indicating how many SDGs have validation errors
- Dialog remains open with validation errors highlighted on the affected SDG cards

### 2. **Visual Validation Feedback**
Each SDG card in the "Selected SDGs" section displays a validation error message when:
- No targets are selected AND
- "I want to opt out" is NOT checked

The error message appears as a red PrimeNG message component below the SDG's targets/indicators section.

### 3. **Automatic Error Clearing**
Validation errors are automatically cleared when:
- User edits the SDG (enters edit mode)
- User updates the SDG with valid configuration
- User removes the SDG from pending selections
- User clears all pending SDGs
- User successfully commits all SDGs (after validation passes)

---

## 🔧 Technical Implementation

### TypeScript Component Changes

#### Modified Button Disable Logic
**CRITICAL CHANGE**: Modified `isSDGConfigurationComplete` computed property to allow incomplete SDGs to be added to pending selections.

**Before** (prevented adding incomplete SDGs):
```typescript
readonly isSDGConfigurationComplete = computed(() => {
  const currentSdg = this.sdgControlValue() || this.sdgControl.value;
  if (!currentSdg) {
    return false;
  }
  
  // Prevented adding if no targets and no opt-out
  const skipTargets = this.skipTargetsControlValue() || this.skipTargetsControl.value;
  if (skipTargets) {
    return true;
  }
  
  const selectedTargetsMap = this.selectedTargets();
  return selectedTargetsMap.size > 0;
});
```

**After** (allows incomplete SDGs - validation happens at commit time):
```typescript
readonly isSDGConfigurationComplete = computed(() => {
  // Only check if SDG is selected - targets/opt-out validation now happens at commit time
  const currentSdg = this.sdgControlValue() || this.sdgControl.value;
  return !!currentSdg;
});
```

**Impact**: 
- "Add SDG to Selection" button: Enabled when SDG is selected (regardless of targets/opt-out)
- "Update SDG Configuration" button: Enabled when SDG is selected (regardless of targets/opt-out)
- Validation now happens when user clicks "Add x SDGs" button (commit time)

#### New Signal Added
```typescript
// Track validation errors for pending SDGs (by index)
sdgValidationErrors = signal<Set<number>>(new Set());
```

#### Validation Functions

**1. Validate Individual SDG**
```typescript
private validateSDG(sdg: OpportunitySDG): boolean {
  // Valid if skip targets is checked
  if (sdg.skipTargetsAndIndicators) {
    return true;
  }
  
  // Valid if at least one target is selected
  return !!(sdg.targets && sdg.targets.length > 0);
}
```

**2. Validate All Pending SDGs**
```typescript
private validateAllPendingSDGs(): number[] {
  const pending = this.pendingSDGSelections();
  const invalidIndices: number[] = [];
  
  pending.forEach((sdg, index) => {
    if (!this.validateSDG(sdg)) {
      invalidIndices.push(index);
    }
  });
  
  return invalidIndices;
}
```

**3. Check if SDG Has Validation Error (Public Helper)**
```typescript
hasValidationError(index: number): boolean {
  return this.sdgValidationErrors().has(index);
}
```

#### Updated Methods

**1. `commitPendingSDGs()` - Validate Before Commit**
```typescript
commitPendingSDGs(): void {
  const pending = this.pendingSDGSelections();

  if (pending.length === 0) {
    return;
  }

  // Validate all pending SDGs
  const invalidIndices = this.validateAllPendingSDGs();
  
  if (invalidIndices.length > 0) {
    // Show validation errors for invalid SDGs
    this.sdgValidationErrors.set(new Set(invalidIndices));
    
    // Show error toast
    this.feedbackService.showErrorToast({
      detail: this.translateService.instant(
        invalidIndices.length === 1
          ? 'message.validation.sdgTargetsRequired'
          : 'message.validation.multipleSDGsTargetsRequired',
        { count: invalidIndices.length }
      ),
      summary: this.translateService.instant('message.validation.validationFailed'),
    });
    
    this.cdr.detectChanges();
    return;
  }

  // ... rest of commit logic
  
  // Clear validation errors on successful commit
  this.sdgValidationErrors.set(new Set());
  // ... close dialog
}
```

**2. `removePendingSDG()` - Adjust Validation Indices**
```typescript
removePendingSDG(index: number): void {
  const currentPending = [...this.pendingSDGSelections()];
  currentPending.splice(index, 1);
  this.pendingSDGSelections.set(currentPending);
  
  // Clear validation errors and recalculate for remaining SDGs
  const currentErrors = this.sdgValidationErrors();
  const newErrors = new Set<number>();
  
  // Adjust indices for remaining errors
  currentErrors.forEach(errorIndex => {
    if (errorIndex < index) {
      newErrors.add(errorIndex);
    } else if (errorIndex > index) {
      newErrors.add(errorIndex - 1);
    }
    // Skip errorIndex === index (the removed SDG)
  });
  
  this.sdgValidationErrors.set(newErrors);
  this.cdr.detectChanges();
}
```

**3. `clearPendingSDGs()` - Reset Validation Errors**
```typescript
clearPendingSDGs(): void {
  this.pendingSDGSelections.set([]);
  this.sdgValidationErrors.set(new Set());
  this.resetSDGConfiguration();
  this.cdr.detectChanges();
}
```

**4. `editPendingSDG()` - Clear Error on Edit**
```typescript
editPendingSDG(index: number): void {
  // ... existing edit logic
  
  // Clear validation error for this SDG when entering edit mode
  const currentErrors = this.sdgValidationErrors();
  if (currentErrors.has(index)) {
    const newErrors = new Set(currentErrors);
    newErrors.delete(index);
    this.sdgValidationErrors.set(newErrors);
  }
  
  // ... rest of edit logic
}
```

**5. `updatePendingSDG()` - Clear Error on Update**
```typescript
updatePendingSDG(): void {
  // ... existing update logic
  
  this.pendingSDGSelections.set(currentPending);
  
  // Clear validation error for this SDG since it's been updated
  const currentErrors = this.sdgValidationErrors();
  if (currentErrors.has(index)) {
    const newErrors = new Set(currentErrors);
    newErrors.delete(index);
    this.sdgValidationErrors.set(newErrors);
  }
  
  // ... rest of update logic
}
```

---

### HTML Template Changes

**Validation Error Message on SDG Cards**

Added validation error message display in the SDG card template:

```html
<!-- Display Targets and Indicators (condensed) -->
@if (sdg.skipTargetsAndIndicators) {
  <div class="text-xs text-gray-500 italic">
    ({{ "label.opportunity.skipTargetsAndIndicators" | translate }})
  </div>
} @else if (sdg.targets && sdg.targets.length > 0) {
  <div class="mt-2 pl-3 border-l-2 border-unops-primary-200 space-y-1">
    <!-- Targets and indicators display -->
  </div>
}

<!-- Validation Error Message -->
@if (hasValidationError($index)) {
  <div class="mt-3">
    <p-message 
      severity="error" 
      variant="simple"
      styleClass="w-full"
    >
      {{ "message.validation.sdgTargetsOrOptOutRequired" | translate }}
    </p-message>
  </div>
}
```

**Location**: After the targets/indicators section and before the action buttons on each SDG card.

---

### Translation Keys Added

All translation keys added to 4 language files:
- **English** (`en.json`)
- **Spanish** (`span.json`)
- **French** (`fr.json`)
- **Portuguese** (`pt.json`)

#### English Translations
```json
{
  "message.validation.sdgRequired": "Please select an SDG",
  "message.validation.sdgTargetsOrOptOutRequired": "Please select at least one target or check 'I want to opt out' option",
  "message.validation.sdgTargetsRequired": "This SDG requires either at least one target selected or the 'opt out' option checked",
  "message.validation.multipleSDGsTargetsRequired": "{{count}} SDG(s) require either at least one target selected or the 'opt out' option checked. Please review the validation errors shown below.",
  "message.validation.validationFailed": "Validation Failed"
}
```

#### Spanish Translations
```json
{
  "message.validation.sdgRequired": "Por favor seleccione un ODS",
  "message.validation.sdgTargetsOrOptOutRequired": "Por favor seleccione al menos una meta o marque la opción 'Deseo optar por no participar'",
  "message.validation.sdgTargetsRequired": "Este ODS requiere que se seleccione al menos una meta o que se marque la opción 'optar por no participar'",
  "message.validation.multipleSDGsTargetsRequired": "{{count}} ODS requieren que se seleccione al menos una meta o que se marque la opción 'optar por no participar'. Por favor, revise los errores de validación que se muestran a continuación.",
  "message.validation.validationFailed": "Error de Validación"
}
```

#### French Translations
```json
{
  "message.validation.sdgRequired": "Veuillez sélectionner un ODD",
  "message.validation.sdgTargetsOrOptOutRequired": "Veuillez sélectionner au moins une cible ou cocher l'option 'Je souhaite me désengager'",
  "message.validation.sdgTargetsRequired": "Cet ODD nécessite qu'au moins une cible soit sélectionnée ou que l'option 'se désengager' soit cochée",
  "message.validation.multipleSDGsTargetsRequired": "{{count}} ODD nécessitent qu'au moins une cible soit sélectionnée ou que l'option 'se désengager' soit cochée. Veuillez examiner les erreurs de validation affichées ci-dessous.",
  "message.validation.validationFailed": "Échec de la Validation"
}
```

#### Portuguese Translations
```json
{
  "message.validation.sdgRequired": "Por favor, selecione um ODS",
  "message.validation.sdgTargetsOrOptOutRequired": "Por favor, selecione pelo menos uma meta ou marque a opção 'Desejo não participar'",
  "message.validation.sdgTargetsRequired": "Este ODS requer que pelo menos uma meta seja selecionada ou que a opção 'não participar' seja marcada",
  "message.validation.multipleSDGsTargetsRequired": "{{count}} ODS requerem que pelo menos uma meta seja selecionada ou que a opção 'não participar' seja marcada. Por favor, revise os erros de validação mostrados abaixo.",
  "message.validation.validationFailed": "Falha na Validação"
}
```

---

## 📊 Validation Logic Flow

```
User clicks "Add X SDGs" button
        │
        ├─> Validate all pending SDGs
        │
        ├─> Any invalid SDGs found?
        │   │
        │   ├─ YES → Show validation errors
        │   │         ├─> Mark invalid SDG indices in signal
        │   │         ├─> Display error toast notification
        │   │         ├─> Show error messages on SDG cards
        │   │         └─> Keep dialog open
        │   │
        │   └─ NO  → Commit all SDGs
        │             ├─> Clear validation errors
        │             ├─> Emit updated opportunity
        │             ├─> Show success toast
        │             └─> Close dialog
        │
        └─> Done
```

### Individual SDG Validation Logic

```
START
  │
  ├─> Is "Skip targets and indicators" checked?
  │   ├─ YES → VALID ✅
  │   └─ NO  → Continue
  │
  └─> Are any targets selected?
      ├─ YES → VALID ✅
      └─ NO  → INVALID ❌
```

---

## 🎨 User Experience

### Scenario 1: All SDGs Valid
**User Actions:**
1. Opens SDG dialog
2. Configures 3 SDGs with proper targets or opt-out
3. Clicks "Add 3 SDGs"

**Result:**
- ✅ All SDGs committed successfully
- ✅ Success toast shown
- ✅ Dialog closes
- ✅ No validation errors displayed

### Scenario 2: Some SDGs Invalid
**User Actions:**
1. Opens SDG dialog
2. Configures 3 SDGs:
   - SDG 1: Has targets ✅
   - SDG 2: No targets, no opt-out ❌
   - SDG 3: Opt-out checked ✅
3. Clicks "Add 3 SDGs"

**Result:**
- ❌ Validation fails
- ❌ Error toast appears: "1 SDG requires either at least one target selected or the 'opt out' option checked. Please review the validation errors shown below."
- ❌ Red error message appears on SDG 2 card
- ❌ Dialog remains open
- ✅ User can edit SDG 2 to fix the issue
- ✅ User can remove SDG 2 if not needed
- ✅ User can add targets or check opt-out for SDG 2

### Scenario 3: Multiple SDGs Invalid
**User Actions:**
1. Opens SDG dialog
2. Configures 4 SDGs:
   - SDG 1: No targets, no opt-out ❌
   - SDG 2: Has targets ✅
   - SDG 3: No targets, no opt-out ❌
   - SDG 4: Opt-out checked ✅
3. Clicks "Add 4 SDGs"

**Result:**
- ❌ Validation fails
- ❌ Error toast appears: "2 SDG(s) require either at least one target selected or the 'opt out' option checked. Please review the validation errors shown below."
- ❌ Red error messages appear on SDG 1 and SDG 3 cards
- ❌ Dialog remains open
- ✅ User can fix both SDGs before committing

### Scenario 4: User Fixes Validation Errors
**User Actions:**
1. Sees validation errors on 2 SDGs
2. Clicks "Edit" on first invalid SDG
3. Selects targets or checks opt-out
4. Clicks "Update"
5. Repeats for second invalid SDG
6. Clicks "Add X SDGs"

**Result:**
- ✅ Validation errors cleared as SDGs are updated
- ✅ All SDGs now valid
- ✅ Commit succeeds
- ✅ Success toast shown
- ✅ Dialog closes

---

## ✅ Benefits

### 1. **Data Quality**
- Ensures all SDG configurations are meaningful
- Prevents incomplete data from being saved
- Forces users to make explicit choices (targets or opt-out)

### 2. **User Guidance**
- Clear visual feedback on what needs to be fixed
- Specific error messages on each invalid SDG
- Toast notification summarizes validation state

### 3. **Flexible Workflow**
- Users can configure multiple SDGs before validation
- Can fix errors without losing progress
- Can remove invalid SDGs if not needed

### 4. **Consistent UX**
- Matches validation patterns used elsewhere in the app
- Uses standard PrimeNG message components
- Follows Angular best practices with signals

---

## 🧪 Testing Checklist

### Validation Logic Tests
- [ ] SDG with targets selected is valid
- [ ] SDG with opt-out checked is valid
- [ ] SDG with neither targets nor opt-out is invalid
- [ ] Multiple valid SDGs commit successfully
- [ ] Single invalid SDG prevents commit
- [ ] Multiple invalid SDGs all flagged

### UI Interaction Tests
- [ ] Validation error message displays on invalid SDG card
- [ ] Error toast shows correct count of invalid SDGs
- [ ] Edit button clears validation error for that SDG
- [ ] Update button clears validation error after fix
- [ ] Remove button clears validation error for that SDG
- [ ] Clear All button resets all validation errors
- [ ] Successful commit clears all validation errors

### Translation Tests
- [ ] English messages display correctly
- [ ] Spanish messages display correctly
- [ ] French messages display correctly
- [ ] Portuguese messages display correctly
- [ ] Plural forms handled correctly (1 SDG vs X SDGs)

### Edge Cases
- [ ] Validation works with only 1 pending SDG
- [ ] Validation works with many pending SDGs
- [ ] Remove SDG correctly adjusts validation indices
- [ ] Edit different SDGs maintains correct validation state

---

## 📝 Files Modified

### TypeScript Component
- **File**: `UNOPS.PAO.ClientApp/src/app/features/partnerships/opportunities/components/opportunity/view/sections/why/opportunity-why-section.component.ts`
- **Changes**:
  - **MODIFIED** `isSDGConfigurationComplete` computed property (simplified to only check if SDG is selected)
  - Added `sdgValidationErrors` signal
  - Added `validateSDG()` private method
  - Added `validateAllPendingSDGs()` private method
  - Added `hasValidationError()` public method
  - Updated `commitPendingSDGs()` method
  - Updated `removePendingSDG()` method
  - Updated `clearPendingSDGs()` method
  - Updated `editPendingSDG()` method
  - Updated `updatePendingSDG()` method

### HTML Template
- **File**: `UNOPS.PAO.ClientApp/src/app/features/partnerships/opportunities/components/opportunity/view/sections/why/opportunity-why-section.component.html`
- **Changes**:
  - Added validation error message section to SDG cards

### Translation Files
- **Files**:
  - `UNOPS.PAO.ClientApp/src/assets/i18n/en.json`
  - `UNOPS.PAO.ClientApp/src/assets/i18n/span.json`
  - `UNOPS.PAO.ClientApp/src/assets/i18n/fr.json`
  - `UNOPS.PAO.ClientApp/src/assets/i18n/pt.json`
- **Changes**:
  - Added 5 new translation keys for validation messages

---

## 🎉 Summary

Successfully implemented comprehensive SDG validation that:
- ✅ **Changed validation approach from preventive to reactive**
  - Previously: Button disabled to prevent adding incomplete SDGs
  - Now: Button enabled, validation runs at commit time with clear error feedback
- ✅ Validates all pending SDGs before commit
- ✅ Displays clear validation errors on affected SDG cards
- ✅ Shows helpful toast notifications
- ✅ Automatically manages validation state during user interactions
- ✅ Supports all 4 application languages
- ✅ Prevents incomplete SDG data from being saved
- ✅ Maintains excellent user experience with clear feedback
- ✅ Allows users to configure multiple SDGs quickly and fix issues later

### Validation Approach Comparison

**Old Approach (Preventive)**:
- ❌ Button disabled when targets/opt-out not selected
- ❌ User blocked from adding SDG to pending selections
- ❌ Must fix each SDG before adding next one
- ❌ No way to quickly add multiple SDGs and fix later

**New Approach (Reactive)**:
- ✅ Button enabled when SDG is selected
- ✅ User can add incomplete SDGs to pending selections
- ✅ Can configure multiple SDGs quickly
- ✅ Validation runs at commit time with clear error messages
- ✅ Can fix all issues before final commit

The validation enhancement ensures data quality while providing users with a more flexible workflow and clear guidance on how to fix any issues before committing their SDG selections.

