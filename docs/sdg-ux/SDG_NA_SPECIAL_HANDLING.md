# SDG "N/A" (No Contribution to SDGs) - Special Handling

## 🎯 Purpose
Implement special handling for the "N/A" SDG option which represents "No contribution to the SDGs". This option is mutually exclusive with other SDG selections.

## ✨ Key Features

### 1. **Adding "N/A" SDG Clears Others**
When user selects "N/A" SDG and clicks "Add to Selection":
- If other SDGs exist in pending selections → Show confirmation dialog
- If user confirms → Clear all SDGs and add only "N/A" as Primary
- "N/A" always set as Primary, always has `skipTargetsAndIndicators: true`

### 2. **Adding Regular SDG Removes "N/A"**
When user selects a regular SDG while "N/A" exists:
- Automatically remove "N/A" from pending selections
- Show info toast that "N/A" was removed
- Continue with adding the regular SDG

### 3. **No Validation Required for "N/A"**
The "N/A" SDG bypasses the targets/opt-out validation:
- Does not require targets selection
- Does not require "I want to opt out" checkbox
- Always considered valid

### 4. **Editing Behavior**
When editing SDGs:
- Changing TO "N/A" → Show confirmation, clear others
- Changing FROM "N/A" to regular SDG → Show info message

---

## 🔧 Technical Implementation

### TypeScript Component Changes

#### 1. Modified `addSDGToPendingSelection()` Method

```typescript
addSDGToPendingSelection(): void {
  const sdg = this.sdgControl.value;
  let currentPending = [...this.pendingSDGSelections()];
  const isNASDG = sdg.sdgId === 'N/A';

  // SPECIAL HANDLING FOR "N/A" SDG
  if (isNASDG) {
    if (currentPending.length > 0) {
      // Show confirmation dialog
      this.feedbackService.showConfirmDialog(
        {
          summary: 'confirmation.clearAllSDGs',
          detail: 'message.opportunity.addingNASDGWillClearOthers',
        },
        () => {
          this.addNASDG(sdg, opp);
        }
      );
      return;
    } else {
      this.addNASDG(sdg, opp);
      return;
    }
  }

  // REGULAR SDG: Remove N/A if it exists
  const naIndex = currentPending.findIndex((s) => s.sdgId === 'N/A');
  if (naIndex !== -1) {
    currentPending.splice(naIndex, 1);
    this.feedbackService.showInfoToast({
      detail: 'message.opportunity.naSDGRemovedWhenAddingOthers',
    });
  }

  // ... continue with regular SDG logic
}
```

#### 2. New `addNASDG()` Helper Method

```typescript
private addNASDG(sdg: SDG, opp: Opportunity): void {
  const naSDG: OpportunitySDG = {
    id: 0,
    opportunityId: opp.id!,
    sdgId: sdg.sdgId || '',
    sdgDatabaseId: sdg.id,
    sdgNumber: sdg.sdgNumber || '',
    sdgName: sdg.name,
    isPrimary: true, // N/A is always Primary
    skipTargetsAndIndicators: true, // N/A doesn't need targets
    notes: null,
    targets: [],
  };

  // Clear all pending selections and add only N/A
  this.pendingSDGSelections.set([naSDG]);
  this.sdgValidationErrors.set(new Set());
  this.resetSDGConfiguration();

  this.feedbackService.showSuccessToast({
    detail: 'message.opportunity.naSDGAdded',
  });
}
```

#### 3. Modified `updatePendingSDG()` Method

```typescript
updatePendingSDG(): void {
  const sdg = this.sdgControl.value;
  const isNASDG = sdg.sdgId === 'N/A';

  // SPECIAL HANDLING: Changing TO "N/A" SDG
  if (isNASDG) {
    if (currentPending.length > 1) {
      // Show confirmation
      this.feedbackService.showConfirmDialog(
        {
          summary: 'confirmation.clearAllSDGs',
          detail: 'message.opportunity.changingToNASDGWillClearOthers',
        },
        () => {
          this.addNASDG(sdg, opp);
        }
      );
      return;
    } else {
      this.addNASDG(sdg, opp);
      return;
    }
  }

  // SPECIAL HANDLING: Changing FROM "N/A" to a regular SDG
  const originalSDG = currentPending[index];
  if (originalSDG.sdgId === 'N/A' && !isNASDG) {
    this.feedbackService.showInfoToast({
      detail: 'message.opportunity.replacingNASDG',
    });
  }

  // ... continue with regular update logic
}
```

#### 4. Modified `validateSDG()` Method

```typescript
private validateSDG(sdg: OpportunitySDG): boolean {
  // "N/A" SDG is always valid (doesn't need targets or opt-out)
  if (sdg.sdgId === 'N/A') {
    return true;
  }
  
  // Regular validation for other SDGs
  if (sdg.skipTargetsAndIndicators) {
    return true;
  }
  
  return !!(sdg.targets && sdg.targets.length > 0);
}
```

---

## 🌍 Translation Keys Added

All translation keys added to 4 language files (en.json, span.json, fr.json, pt.json):

### English
```json
{
  "message.opportunity.naSDGAdded": "'No contribution to the SDGs' option added successfully",
  "message.opportunity.addingNASDGWillClearOthers": "Adding 'No contribution to the SDGs' will remove all other SDGs, targets, and indicators you have selected. Do you want to continue?",
  "message.opportunity.changingToNASDGWillClearOthers": "Changing to 'No contribution to the SDGs' will remove all other SDGs, targets, and indicators. Do you want to continue?",
  "message.opportunity.naSDGRemovedWhenAddingOthers": "'No contribution to the SDGs' option has been removed because you are adding specific SDGs.",
  "message.opportunity.replacingNASDG": "Replacing 'No contribution to the SDGs' with a specific SDG.",
  "confirmation.clearAllSDGs": "Clear All SDGs?"
}
```

### Spanish
```json
{
  "message.opportunity.naSDGAdded": "Opción 'Sin contribución a los ODS' agregada exitosamente",
  "message.opportunity.addingNASDGWillClearOthers": "Agregar 'Sin contribución a los ODS' eliminará todos los demás ODS, metas e indicadores que ha seleccionado. ¿Desea continuar?",
  "message.opportunity.changingToNASDGWillClearOthers": "Cambiar a 'Sin contribución a los ODS' eliminará todos los demás ODS, metas e indicadores. ¿Desea continuar?",
  "message.opportunity.naSDGRemovedWhenAddingOthers": "La opción 'Sin contribución a los ODS' ha sido eliminada porque está agregando ODS específicos.",
  "message.opportunity.replacingNASDG": "Reemplazando 'Sin contribución a los ODS' con un ODS específico.",
  "confirmation.clearAllSDGs": "¿Borrar Todos los ODS?"
}
```

### French
```json
{
  "message.opportunity.naSDGAdded": "Option 'Aucune contribution aux ODD' ajoutée avec succès",
  "message.opportunity.addingNASDGWillClearOthers": "L'ajout de 'Aucune contribution aux ODD' supprimera tous les autres ODD, cibles et indicateurs que vous avez sélectionnés. Voulez-vous continuer?",
  "message.opportunity.changingToNASDGWillClearOthers": "Le passage à 'Aucune contribution aux ODD' supprimera tous les autres ODD, cibles et indicateurs. Voulez-vous continuer?",
  "message.opportunity.naSDGRemovedWhenAddingOthers": "L'option 'Aucune contribution aux ODD' a été supprimée car vous ajoutez des ODD spécifiques.",
  "message.opportunity.replacingNASDG": "Remplacement de 'Aucune contribution aux ODD' par un ODD spécifique.",
  "confirmation.clearAllSDGs": "Effacer Tous les ODD?"
}
```

### Portuguese
```json
{
  "message.opportunity.naSDGAdded": "Opção 'Sem contribuição para os ODS' adicionada com sucesso",
  "message.opportunity.addingNASDGWillClearOthers": "Adicionar 'Sem contribuição para os ODS' removerá todos os outros ODS, metas e indicadores que você selecionou. Deseja continuar?",
  "message.opportunity.changingToNASDGWillClearOthers": "Mudar para 'Sem contribuição para os ODS' removerá todos os outros ODS, metas e indicadores. Deseja continuar?",
  "message.opportunity.naSDGRemovedWhenAddingOthers": "A opção 'Sem contribuição para os ODS' foi removida porque você está adicionando ODS específicos.",
  "message.opportunity.replacingNASDG": "Substituindo 'Sem contribuição para os ODS' por um ODS específico.",
  "confirmation.clearAllSDGs": "Limpar Todos os ODS?"
}
```

---

## 📊 User Workflows

### Scenario 1: Adding "N/A" When Other SDGs Exist

```
User has: [SDG 1, SDG 4, SDG 6]
        │
        ├─> Selects "N/A" from dropdown
        ├─> Clicks "Add to Selection"
        │
        └─> Shows Confirmation Dialog
            ├─> "Clear All SDGs?"
            ├─> "Adding 'No contribution to the SDGs' will remove all other SDGs..."
            │
            ├─> User Clicks "Cancel"
            │   └─> Nothing happens, returns to dialog
            │
            └─> User Clicks "Confirm"
                ├─> Clear all SDGs
                ├─> Add N/A as Primary
                ├─> Show success toast
                └─> Bottom section shows only: [N/A]
```

### Scenario 2: Adding Regular SDG When "N/A" Exists

```
User has: [N/A]
        │
        ├─> Selects "SDG 1" from dropdown
        ├─> Selects targets
        ├─> Clicks "Add to Selection"
        │
        ├─> Info Toast: "N/A option removed because..."
        ├─> N/A automatically removed
        ├─> SDG 1 added to pending
        │
        └─> Bottom section shows: [SDG 1]
```

### Scenario 3: Adding "N/A" When No SDGs Exist

```
User has: []
        │
        ├─> Selects "N/A" from dropdown
        ├─> Clicks "Add to Selection"
        │
        ├─> No confirmation (no other SDGs to clear)
        ├─> Add N/A as Primary
        ├─> Show success toast
        │
        └─> Bottom section shows: [N/A]
```

### Scenario 4: Editing SDG and Changing to "N/A"

```
User has: [SDG 1, SDG 4, SDG 6]
        │
        ├─> Clicks Edit on SDG 4
        ├─> Changes dropdown to "N/A"
        ├─> Clicks "Update SDG Configuration"
        │
        └─> Shows Confirmation Dialog
            ├─> "Clear All SDGs?"
            ├─> "Changing to 'No contribution to the SDGs'..."
            │
            └─> User Confirms
                ├─> Clear all SDGs
                ├─> Add N/A as Primary
                └─> Bottom section shows: [N/A]
```

### Scenario 5: Editing "N/A" and Changing to Regular SDG

```
User has: [N/A]
        │
        ├─> Clicks Edit on N/A
        ├─> Changes dropdown to "SDG 1"
        ├─> Selects targets
        ├─> Clicks "Update SDG Configuration"
        │
        ├─> Info Toast: "Replacing 'No contribution to the SDGs'..."
        ├─> N/A replaced with SDG 1
        │
        └─> Bottom section shows: [SDG 1]
```

---

## ✅ Validation Behavior

### Regular SDGs
- ❌ **Invalid**: No targets AND no opt-out → Shows validation error
- ✅ **Valid**: Has targets OR opt-out checked

### "N/A" SDG
- ✅ **Always Valid**: Bypasses validation
- No targets required
- No opt-out required
- Can be committed without any configuration

### Validation at Commit Time
```typescript
validateAllPendingSDGs(): number[] {
  const pending = this.pendingSDGSelections();
  const invalidIndices: number[] = [];
  
  pending.forEach((sdg, index) => {
    if (!this.validateSDG(sdg)) {
      invalidIndices.push(index);
    }
  });
  
  return invalidIndices;
}

private validateSDG(sdg: OpportunitySDG): boolean {
  // N/A is always valid
  if (sdg.sdgId === 'N/A') return true;
  
  // Regular validation
  if (sdg.skipTargetsAndIndicators) return true;
  return !!(sdg.targets && sdg.targets.length > 0);
}
```

---

## 🎨 UI Behavior

### "N/A" SDG Display

#### In Dialog (Pending Selections):
```
SDG N/A: No contribution to the SDGs
[Primary Badge]
(I want to opt out as this will be identified if development is pursued.)
```

#### In Read Mode:
```
SDG N/A: No contribution to the SDGs
[Primary Badge]
[Amber Info Box]
  "I want to opt out as this will be identified if development is pursued."
  "Selecting this option will skip target and indicator selection for this SDG."
```

### Button States
- "Add to Selection" button: **Enabled** when "N/A" is selected (no targets needed)
- "Update SDG Configuration" button: **Enabled** when changing to "N/A"

---

## 🧪 Testing Scenarios

### Test 1: Add N/A with Confirmation
**Setup**: 3 SDGs configured
**Steps**:
1. Select "N/A" from dropdown
2. Click "Add to Selection"
3. Confirm dialog

**Expected**:
- ✅ Confirmation dialog shows
- ✅ All 3 SDGs cleared
- ✅ Only N/A remains (as Primary)
- ✅ Success toast shown

### Test 2: Add Regular SDG Removes N/A
**Setup**: N/A configured
**Steps**:
1. Select "SDG 1" from dropdown
2. Select targets
3. Click "Add to Selection"

**Expected**:
- ✅ N/A automatically removed
- ✅ Info toast shown
- ✅ SDG 1 added
- ✅ No confirmation required

### Test 3: N/A Bypasses Validation
**Setup**: No SDGs
**Steps**:
1. Select "N/A" from dropdown
2. Don't select targets or opt-out
3. Click "Add to Selection"
4. Click "Add 1 SDG"

**Expected**:
- ✅ No validation errors
- ✅ N/A committed successfully
- ✅ Saved to opportunity

### Test 4: Edit to N/A with Confirmation
**Setup**: 3 SDGs configured
**Steps**:
1. Edit SDG 2
2. Change to "N/A"
3. Click "Update"
4. Confirm dialog

**Expected**:
- ✅ Confirmation dialog shows
- ✅ All SDGs cleared
- ✅ Only N/A remains

### Test 5: Edit from N/A to Regular SDG
**Setup**: N/A configured
**Steps**:
1. Edit N/A
2. Change to "SDG 1"
3. Select targets
4. Click "Update"

**Expected**:
- ✅ Info toast shown
- ✅ N/A replaced with SDG 1
- ✅ No confirmation required

---

## 📝 Files Modified

### TypeScript Component
- **File**: `UNOPS.PAO.ClientApp/src/app/features/partnerships/opportunities/components/opportunity/view/sections/why/opportunity-why-section.component.ts`
- **Methods Modified**:
  - `addSDGToPendingSelection()` - Added N/A detection and handling
  - `updatePendingSDG()` - Added N/A detection and handling
  - `validateSDG()` - Added N/A bypass
- **Methods Added**:
  - `addNASDG()` - Helper to add N/A and clear others

### Translation Files
- `UNOPS.PAO.ClientApp/src/assets/i18n/en.json` - Added 6 new keys
- `UNOPS.PAO.ClientApp/src/assets/i18n/span.json` - Added 6 new keys
- `UNOPS.PAO.ClientApp/src/assets/i18n/fr.json` - Added 6 new keys
- `UNOPS.PAO.ClientApp/src/assets/i18n/pt.json` - Added 6 new keys

---

## ✅ Summary

Successfully implemented special handling for "N/A" SDG that:
- ✅ Shows confirmation when adding N/A with existing SDGs
- ✅ Clears all SDGs when N/A is confirmed
- ✅ Automatically removes N/A when regular SDG is added
- ✅ Always sets N/A as Primary with skipTargetsAndIndicators
- ✅ Bypasses validation for N/A (no targets/opt-out required)
- ✅ Handles editing scenarios (to/from N/A)
- ✅ Provides clear user feedback with dialogs and toasts
- ✅ Supports all 4 application languages

The "N/A" SDG is now properly handled as a mutually exclusive option that represents "No contribution to the SDGs" with appropriate user warnings and automatic conflict resolution!

