import { Component, OnInit, signal, computed, inject, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { Router } from '@angular/router';
import { Observable, of, catchError, shareReplay, startWith } from 'rxjs';
import { map } from 'rxjs/operators';

// PrimeNG imports
import { DropdownModule } from 'primeng/dropdown';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { InputTextarea } from 'primeng/inputtextarea';
import { CheckboxModule } from 'primeng/checkbox';
import { ProgressSpinnerModule } from 'primeng/progressspinner';
import { ToastModule } from 'primeng/toast';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { TooltipModule } from 'primeng/tooltip';
import { CardModule } from 'primeng/card';
import { DividerModule } from 'primeng/divider';
import { ChipModule } from 'primeng/chip';
import { TagModule } from 'primeng/tag';
import { DialogModule } from 'primeng/dialog';
import { ToggleButtonModule } from 'primeng/togglebutton';
import { Tab, TabList, Tabs } from 'primeng/tabs';

// CDK Drag & Drop
import { CdkDragDrop, moveItemInArray, DragDropModule } from '@angular/cdk/drag-drop';

// Listview components
import { ListviewCardComponent } from '../../../../common/pages/components/listview/card/listview-card.component';
import { ListViewColumn, ListViewConfig } from '../../../../common/pages/components/listview/listview.model';

import { MessageService, ConfirmationService } from 'primeng/api';
import { 
  EntityConfigurationService, 
  EntityDropdownModel, 
  EntityConfigurationDetailsResponse,
  EntityFieldConfigurationDto,
  UpdateEntityConfigurationRequest,
  EntityPermissionsModel,
  RelatedFieldOption
} from '../../services/entity-configuration.service';
import { InteractionIconService } from '../../../../common/services/interaction-icon.service';

/**
 * @uiEntity EntityManager
 * @route /admin/entity-manager
 * @description Advanced administrative interface for configuring entity field visibility, ordering, and permissions. Allows customization of how data is displayed in lists and forms across different entities (Partners, Contacts, Interactions).
 * @capabilities configure_entity_fields, reorder_columns, toggle_field_visibility, manage_field_permissions, customize_display_settings, drag_drop_reordering
 * @synonyms field_configuration, column_management, entity_settings, display_configuration, field_admin
 * @mandatoryFields entity_selection
 * @help_when_stuck Select an entity from the dropdown first, then configure fields using the tabs. Use Field Configuration to show/hide columns, Field Ordering to drag and reorder fields, and Permissions to control access. Changes auto-save as you make them.
 * @common_tasks
 *   - Configuring field visibility: Select entity, go to Field Configuration tab, toggle checkboxes
 *   - Reordering columns: Use Field Ordering tab, drag fields to desired positions
 *   - Managing permissions: Use Permissions tab to control who can see specific fields
 *   - Customizing display: Use Display Settings to configure labels, icons, and appearance
 *   - Testing changes: Save and navigate to the entity list to see your changes applied
 */

@Component({
  selector: 'app-entity-manager',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    TranslateModule,
    DropdownModule,
    ButtonModule,
    InputTextModule,
    InputTextarea,
    CheckboxModule,
    ProgressSpinnerModule,
    ToastModule,
    ConfirmDialogModule,
    TooltipModule,
    CardModule,
    DividerModule,
    ChipModule,
    TagModule,
    DragDropModule,
    DialogModule,
    ToggleButtonModule,
    Tabs,
    TabList,
    Tab,
    ListviewCardComponent
  ],
  providers: [MessageService, ConfirmationService],
  templateUrl: './entity-manager.component.html',
  styleUrls: ['./entity-manager.component.scss']
})
export class EntityManagerComponent implements OnInit {
  private entityConfigService = inject(EntityConfigurationService);
  private messageService = inject(MessageService);
  private confirmationService = inject(ConfirmationService);
  private router = inject(Router);
  private cdr = inject(ChangeDetectorRef);
  private translateService = inject(TranslateService);
  private interactionIconService = inject(InteractionIconService);

  // Auto-save timer for debounced saving
  private autoSaveTimer?: ReturnType<typeof setTimeout>;

  // State signals
  entities = signal<EntityDropdownModel[]>([]);
  selectedEntityName = signal<string>('');
  currentEntityConfig = signal<EntityConfigurationDetailsResponse | null>(null);
  originalEntityConfig = signal<EntityConfigurationDetailsResponse | null>(null);
  
  // Loading states
  entitiesLoading = signal<boolean>(false);
  configLoading = signal<boolean>(false);
  saving = signal<boolean>(false);
  fieldSaving = signal<boolean>(false);
  configSaving = signal<boolean>(false);
  autoSaving = signal<boolean>(false);
  permissionsLoading = signal<boolean>(true);

  // Additional loading state
  loading = computed(() => this.entitiesLoading() || this.configLoading());

  // Working fields state for field management
  workingFields = signal<EntityFieldConfigurationDto[]>([]);

  // Effect to sync workingFields when currentEntityConfig changes
  private syncWorkingFieldsEffect = computed(() => {
    const config = this.currentEntityConfig();
    if (config && config.fields) {
      this.workingFields.set([...config.fields]);
    } else {
      this.workingFields.set([]);
    }
  });

  // Permissions
  permissions = signal<EntityPermissionsModel>({
    entity: 'EntityManager',
    canCreate: false,
    canRead: false,
    canUpdate: false,
    canDelete: false,
    canExport: false,
    canImport: false
  });

  // UI state
  hasUnsavedChanges = signal<boolean>(false);
  editingFieldId = signal<number | undefined | null>(null);
  showListViewPanel = signal<boolean>(false);
  showFieldEditPanel = signal<boolean>(false);

  // Field editing state
  editingField = signal<EntityFieldConfigurationDto | null>(null);

  // Entity configuration editing state
  entityConfigForm = signal<UpdateEntityConfigurationRequest | null>(null);

  // Dialog states
  showFieldEditDialog = signal<boolean>(false);
  showEntityConfigDialog = signal<boolean>(false);

  // Dialog title computed property
  fieldDialogTitle = computed(() => {
    const field = this.editingField();
    return field?.id ? `Edit Field: ${field.fieldName}` : 'Add New Field';
  });

  // Filtering state
  searchFilter = signal<string>('');
  showOnlyListViewFields = signal<boolean>(false);
  showListViewFieldsOnTop = signal<boolean>(false);
  selectedField = signal<EntityFieldConfigurationDto | null>(null);

  // Sample data for template preview
  sampleData = signal<any>(null);

  // IDE-style autocompletion for templates
  templateAvailableFields = signal<string[]>([]);
  templateFilteredFields = signal<string[]>([]);
  showTemplateSuggestions = signal<boolean>(false);
  selectedSuggestionIndex = signal<number>(0);

  // Data type options for dropdown
  dataTypeOptions = [
    { label: 'String', value: 'string' },
    { label: 'Integer', value: 'int' },
    { label: 'Boolean', value: 'boolean' },
    { label: 'DateTime', value: 'datetime' },
    { label: 'Date', value: 'date' },
    { label: 'Enum', value: 'enum' },
    { label: 'Contact', value: 'Contact' },
    { label: 'Partner', value: 'Partner' },
    { label: 'Contact[]', value: 'Contact[]' },
    { label: 'Partner[]', value: 'Partner[]' },
    { label: 'Document[]', value: 'Document[]' },
    { label: 'Project[]', value: 'Project[]' },
    { label: 'OrganizationHierarchy', value: 'OrganizationHierarchy' },
    { label: 'PartnerTree', value: 'PartnerTree' },
    { label: 'PartnerTree[]', value: 'PartnerTree[]' },
    { label: 'String[]', value: 'string[]' }
  ];

  // Column type options for dropdown
  getColumnTypeOptions() {
    return [
      { label: 'Text', value: 'text' },
      { label: 'Avatar', value: 'avatar' },
      { label: 'Template', value: 'template' },
      { label: 'Multiple Avatars', value: 'multiple-avatars' }
    ];
  }

  // Computed values
  hasAccessToManage = computed(() => this.permissions().canUpdate);
  canViewOnly = computed(() => this.permissions().canRead && !this.permissions().canUpdate);
  entityOptions = computed(() => 
    this.entities().map(entity => ({ 
      label: entity.entityName, 
      value: entity.entityName 
    }))
  );

  // Computed values for responsive tabs
  entityTabs = computed(() => 
    this.entities().map(entity => ({
      label: entity.entityName,
      value: entity.entityName,
      translatedLabel: entity.entityName
    }))
  );

  // List view management computed values
  availableFields = computed(() => 
    this.workingFields().filter(field => !field.showInListView && field.isActive)
  );
  
  listViewFields = computed(() => 
    this.workingFields()
      .filter(field => field.showInListView && field.isActive)
      .sort((a, b) => (a.listViewOrder ?? 0) - (b.listViewOrder ?? 0))
  );

  // Preview configuration for the card
  previewCardColumns = computed(() => {
    const fields = this.getListViewFields();
    if (!fields || fields.length === 0) {
      return [];
    }

    return fields.map(field => this.convertFieldToColumn(field));
  });

  previewCardConfig = computed<ListViewConfig>(() => ({
    pageSize: 1,
    enableSelection: false,
    enablePagination: false,
    enableSorting: false,
    enableSearch: false,
    enableExport: false,
    showViewModeToggle: false,
    defaultViewMode: 'card',
    forceMobileMode: true
  }));

  previewCardData = computed(() => {
    const sample = this.sampleData();
    return sample ? [sample] : [];
  });

  // Computed property to check if preview should be shown
  showCardPreview = computed(() => {
    const hasFields = this.getListViewFields().length > 0;
    const hasSampleData = this.sampleData() !== null;
    return hasFields && hasSampleData && this.selectedEntityName() && !this.configLoading();
  });

  // Filtered fields computed value
  filteredFields = computed(() => {
    let fields = this.workingFields();
    
    // Apply search filter
    const searchTerm = this.searchFilter().toLowerCase();
    if (searchTerm) {
      fields = fields.filter(field => 
        field.fieldName.toLowerCase().includes(searchTerm) ||
        field.dataType.toLowerCase().includes(searchTerm) ||
        (field.description && field.description.toLowerCase().includes(searchTerm))
      );
    }
    
    // Apply list view filter
    if (this.showOnlyListViewFields()) {
      fields = fields.filter(field => field.showInListView);
    }
    
    return fields;
  });

  // Cache for related entity fields to prevent infinite API calls
  private relatedFieldsCache = new Map<string, Observable<any[]>>();

  // Working copies for editing
  workingEntityConfig = signal<UpdateEntityConfigurationRequest>({
    id: 0,
    entityName: '',
    tableName: '',
    description: '',
    isActive: true,
    enableChangeLog: false
  });

  ngOnInit() {
    this.loadPermissions();
    this.resetDialogStates();
  }

  private resetDialogStates() {
    this.editingFieldId.set(null);
    this.editingField.set(null);
    this.showListViewPanel.set(false);
    this.showFieldEditPanel.set(false);
    this.showFieldEditDialog.set(false);
    this.showEntityConfigDialog.set(false);
    this.entityConfigForm.set(null);
  }

  private loadPermissions() {
    this.permissionsLoading.set(true);
    this.entityConfigService.getEntityPermissions().subscribe({
      next: (permissions) => {
        this.permissions.set(permissions);
        this.permissionsLoading.set(false);
        
        if (!permissions.canRead) {
          this.messageService.add({
            severity: 'error',
            summary: 'Access Denied',
            detail: 'You do not have permission to access Entity Management'
          });
          this.router.navigate(['/']);
          return;
        }
        
        this.loadEntities();
      },
      error: (error) => {
        console.error('Error loading permissions:', error);
        this.permissionsLoading.set(false);
        this.messageService.add({
          severity: 'error',
          summary: 'Error',
          detail: 'Failed to load permissions'
        });
      }
    });
  }

  private loadEntities() {
    this.entitiesLoading.set(true);
    this.entityConfigService.getEntities().subscribe({
      next: (entities) => {
        // Filter out PartnerTree from the entities list
        const filteredEntities = entities.filter(entity => entity.entityName !== 'PartnerTree');
        this.entities.set(filteredEntities);
        this.entitiesLoading.set(false);
        
        // Sélectionner automatiquement la première entité si aucune n'est sélectionnée
        if (filteredEntities.length > 0 && !this.selectedEntityName()) {
          this.selectedEntityName.set(filteredEntities[0].entityName);
          this.onEntityChange();
        }
      },
      error: (error) => {
        console.error('Error loading entities:', error);
        this.entitiesLoading.set(false);
        this.messageService.add({
          severity: 'error',
          summary: 'Error',
          detail: 'Failed to load entities'
        });
      }
    });
  }

  onEntityChange() {
    const entityName = this.selectedEntityName();
    
    this.resetDialogStates();
    
    if (!entityName) {
      this.currentEntityConfig.set(null);
      this.originalEntityConfig.set(null);
      this.workingFields.set([]);
      this.relatedFieldsCache.clear();
      this.sampleData.set(null);
      return;
    }

    this.relatedFieldsCache.clear();
    this.loadEntityConfiguration(entityName);
    // Note: loadSampleData is called in loadEntityConfiguration after config is loaded
  }

  // Method to handle tab selection (non-routing)
  onEntityTabChange(entityName: string): void {
    if (entityName !== this.selectedEntityName()) {
      this.selectedEntityName.set(entityName);
      this.onEntityChange();
      this.loadSampleData();
    }
  }
  
  // Method to handle mobile dropdown change
  onEntityDropdownChange(event: any): void {
    const selectedEntity = event.value;
    if (selectedEntity && selectedEntity !== this.selectedEntityName()) {
      this.selectedEntityName.set(selectedEntity);
      this.onEntityChange();
      this.loadSampleData();
    }
  }
  
  // Get current selected entity for dropdown
  getSelectedEntityForDropdown(): any {
    return this.entityTabs().find(tab => tab.value === this.selectedEntityName());
  }

  private loadEntityConfiguration(entityName: string) {
    this.configLoading.set(true);
    this.entityConfigService.getEntityConfiguration(entityName).subscribe({
      next: (config) => {
        this.currentEntityConfig.set(config);
        this.originalEntityConfig.set(JSON.parse(JSON.stringify(config)));
        
        this.workingEntityConfig.set({
          id: config.id!,
          entityName: config.entityName,
          tableName: config.tableName || '',
          description: config.description || '',
          isActive: config.isActive,
          enableChangeLog: config.enableChangeLog
        });
        
        const sortedFields = [...config.fields].sort((a, b) => a.displayOrder - b.displayOrder);
        this.workingFields.set(sortedFields);
        
        this.configLoading.set(false);
        this.hasUnsavedChanges.set(false);
        
        // Load sample data for template preview
        this.loadSampleData();
      },
      error: (error) => {
        console.error('Error loading entity configuration:', error);
        this.configLoading.set(false);
        this.messageService.add({
          severity: 'error',
          summary: 'Error',
          detail: 'Failed to load entity configuration'
        });
      }
    });
  }

  onEntityConfigChange() {
    this.hasUnsavedChanges.set(true);
  }

  onFieldDrop(event: CdkDragDrop<EntityFieldConfigurationDto[]>) {
    const fields = [...this.workingFields()];
    moveItemInArray(fields, event.previousIndex, event.currentIndex);
    
    fields.forEach((field, index) => {
      field.displayOrder = index + 1;
    });
    
    this.workingFields.set(fields);
    this.hasUnsavedChanges.set(true);
  }

  getDataTypeSeverity(dataType: string): "success" | "info" | "warn" | "secondary" | "contrast" | "danger" | undefined {
    switch (dataType.toLowerCase()) {
      case 'int':
      case 'integer':
        return 'info';
      case 'string':
        return 'success';
      case 'boolean':
        return 'warn';
      case 'datetime':
      case 'date':
        return 'danger';
      case 'enum':
        return 'contrast';
      default:
        return 'secondary';
    }
  }

  onFieldShowInListViewChange(fieldId: number | undefined, showInListView: boolean) {
    const fields = this.workingFields();
    const fieldIndex = fields.findIndex(f => f.id === fieldId);
    if (fieldIndex !== -1) {
      const updatedFields = [...fields];
      
      if (showInListView) {
        const currentListViewFields = fields.filter(f => f.showInListView && f.listViewOrder != null);
        
        // Check if we already have 5 fields in list view
        if (currentListViewFields.length >= 5) {
          this.messageService.add({
            severity: 'warn',
            summary: 'Maximum Fields Reached',
            detail: 'You can only have a maximum of 5 fields in the List View. Please remove a field before adding a new one.',
            life: 5000
          });
          return;
        }
        
        const maxListViewOrder = currentListViewFields.length > 0
          ? Math.max(...currentListViewFields.map(f => f.listViewOrder!))
          : 0;
        
        updatedFields[fieldIndex] = {
          ...updatedFields[fieldIndex],
          showInListView: true,
          listViewOrder: maxListViewOrder + 1
        };

        // Show success message
        const field = updatedFields[fieldIndex];
        this.messageService.add({
          severity: 'success',
          summary: 'Field Added',
          detail: `${field.fieldName} has been added to the List View`,
          life: 3000
        });
      } else {
        updatedFields[fieldIndex] = {
          ...updatedFields[fieldIndex],
          showInListView: false,
          listViewOrder: undefined
        };
        
        const remainingListViewFields = updatedFields
          .filter(f => f.showInListView && f.id !== fieldId)
          .sort((a, b) => (a.listViewOrder ?? 0) - (b.listViewOrder ?? 0));
        
        remainingListViewFields.forEach((field, index) => {
          field.listViewOrder = index + 1;
        });

        // Show success message
        const field = updatedFields[fieldIndex];
        this.messageService.add({
          severity: 'success',
          summary: 'Field Removed',
          detail: `${field.fieldName} has been removed from the List View`,
          life: 3000
        });
      }
      
      this.workingFields.set(updatedFields);
      this.hasUnsavedChanges.set(true);
      this.scheduleAutoSave();
    }
  }

  moveFieldToListView(fieldId: number | undefined) {
    this.onFieldShowInListViewChange(fieldId, true);
  }

  removeFieldFromListView(fieldId: number | undefined) {
    this.onFieldShowInListViewChange(fieldId, false);
  }

  onListViewFieldDrop(event: CdkDragDrop<EntityFieldConfigurationDto[]>) {
    const listViewFields = [...this.listViewFields()];
    moveItemInArray(listViewFields, event.previousIndex, event.currentIndex);
    
    const allFields = [...this.workingFields()];
    listViewFields.forEach((field, index) => {
      const fieldIndex = allFields.findIndex(f => f.id === field.id);
      if (fieldIndex !== -1) {
        allFields[fieldIndex] = {
          ...allFields[fieldIndex],
          listViewOrder: index + 1
        };
      }
    });
    
    this.workingFields.set(allFields);
    this.hasUnsavedChanges.set(true);
    this.scheduleAutoSave();
  }

  // TrackBy functions for ngFor performance
  trackByFieldId(index: number, field: EntityFieldConfigurationDto): number | string {
    return field.id ?? `temp-${field.fieldName}-${index}`;
  }

  trackByFieldIdForList(index: number, field: EntityFieldConfigurationDto): number | string {
    return field.id ?? `temp-list-${field.fieldName}-${index}`;
  }

  trackByFieldIdForAvailable(index: number, field: EntityFieldConfigurationDto): number | string {
    return field.id ?? `temp-available-${field.fieldName}-${index}`;
  }

  // Helper method to check if a field is a relationship field
  isRelationshipField(dataType: string): boolean {
    const relationshipTypes = ['Partner', 'Contact', 'PartnerTree', 'OrganizationHierarchy', 'Interaction', 'Contact[]', 'Partner[]', 'Document[]', 'Project[]', 'PartnerTree[]', 'string[]'];
    return relationshipTypes.includes(dataType);
  }

  // Helper method to get data type label for display
  getDataTypeLabel(dataType: string): string {
    const option = this.dataTypeOptions.find(opt => opt.value === dataType);
    return option ? option.label : dataType;
  }

  // Get available display properties for a related entity type
  getRelatedEntityFields(entityType: string): Observable<RelatedFieldOption[]> {
    return this.entityConfigService.getRelatedEntityFields(entityType);
  }


  // Helper method to get dropdown options for display field path (for ALL field types)
  getRelatedDisplayOptions(dataType: string): Observable<any[]> {
    const currentEntityName = this.selectedEntityName();
    if (!currentEntityName) {
      return of([]);
    }

    if (this.isRelationshipField(dataType)) {
      // For relationship fields, use the context-aware method
      return this.getEntityFieldOptionsForDataType(dataType, currentEntityName);
    } else {
      // For simple fields, use the current entity's fields
      return this.getEntityFieldOptions(currentEntityName, true);
    }
  }

  // Get field options for a specific entity
  private getEntityFieldOptions(entityType: string, isSameEntity: boolean = false): Observable<any[]> {
    const cacheKey = `display_${entityType}_${isSameEntity}`;
    if (this.relatedFieldsCache.has(cacheKey)) {
      return this.relatedFieldsCache.get(cacheKey)! as Observable<any[]>;
    }
    
    const options$ = this.getRelatedEntityFields(entityType).pipe(
      map(options => options.map(opt => {
        const fieldPath = isSameEntity 
          ? opt.value
          : opt.fieldPath || `${entityType.toLowerCase()}.${opt.value}`;
        
        return {
          label: `${entityType} - ${opt.label}`,
          value: fieldPath
        };
      })),
      catchError(() => of([])),
      startWith([]),
      shareReplay(1)
    );
    
    this.relatedFieldsCache.set(cacheKey, options$);
    return options$;
  }

  // Get field options for a specific data type in context of an entity
  private getEntityFieldOptionsForDataType(dataType: string, contextEntityName: string): Observable<any[]> {
    const cacheKey = `datatype_${dataType}_${contextEntityName}`;
    if (this.relatedFieldsCache.has(cacheKey)) {
      return this.relatedFieldsCache.get(cacheKey)! as Observable<any[]>;
    }
    
    const options$ = this.entityConfigService.getFieldOptionsForDataType(dataType, contextEntityName).pipe(
      map(options => options.map(opt => ({
        label: `${dataType} - ${opt.label}`,
        value: opt.fieldPath
      }))),
      catchError(() => of([])),
      startWith([]),
      shareReplay(1)
    );
    
    this.relatedFieldsCache.set(cacheKey, options$);
    return options$;
  }


  // Handle enable change log checkbox change
  onEnableChangeLogChange(event: any) {
    const field = this.editingField();
    if (!field) return;

    // If enabling field change log, automatically enable entity change log
    if (event.checked && field.enableChangeLog) {
      const entityConfig = this.workingEntityConfig();
      if (!entityConfig.enableChangeLog) {
        this.workingEntityConfig.set({
          ...entityConfig,
          enableChangeLog: true
        });
        this.hasUnsavedChanges.set(true);
        
        // Show a notification to inform the user
        this.messageService.add({
          severity: 'info',
          summary: 'Entity Change Log Enabled',
          detail: 'Entity change log has been automatically enabled since field change log is now enabled.'
        });
      }
    }
  }

  // Handle template pattern changes from text input
  onTemplatePatternChange(fieldId: number | undefined, value: string) {
    const fields = this.workingFields();
    const fieldIndex = fields.findIndex(f => f.id === fieldId);
    if (fieldIndex !== -1) {
      const updatedFields = [...fields];
      updatedFields[fieldIndex] = {
        ...updatedFields[fieldIndex],
        displayTemplate: value
      };
      this.workingFields.set(updatedFields);
      this.hasUnsavedChanges.set(true);
      this.scheduleAutoSave();
    }
  }

  onRelatedDisplayPropertyChange(fieldId: number | undefined, value: string) {
    const fields = this.workingFields();
    const fieldIndex = fields.findIndex(f => f.id === fieldId);
    if (fieldIndex !== -1) {
      const updatedFields = [...fields];
      
      const field = updatedFields[fieldIndex];
      if (this.isRelationshipField(field.dataType)) {
        const baseEntityType = field.dataType.replace('[]', '');
        const currentEntityName = this.selectedEntityName();
        const isSameEntity = baseEntityType.toLowerCase() === currentEntityName.toLowerCase();
        
        this.getRelatedEntityFields(baseEntityType).subscribe(options => {
          const selectedOption = options.find(opt => opt.value === value);
          
          // Generate template path instead of fieldPath
          const templatePath = isSameEntity 
            ? `{${value}}`
            : `{${selectedOption?.fieldPath || `${baseEntityType.toLowerCase()}.${value}`}}`;
          
          updatedFields[fieldIndex] = {
            ...updatedFields[fieldIndex],
            relatedDisplayProperty: value,
            displayTemplate: selectedOption?.isTemplate ? selectedOption.templatePattern : templatePath,
            listViewType: 'template' // Always use template since we're using displayTemplate
          };
          
          this.workingFields.set(updatedFields);
          this.hasUnsavedChanges.set(true);
          this.scheduleAutoSave();
        });
      }
    }
  }

  onListViewConfigChange(fieldId: number | undefined, property: string, value: any) {
    const fields = this.workingFields();
    const fieldIndex = fields.findIndex(f => f.id === fieldId);
    if (fieldIndex !== -1) {
      const updatedFields = [...fields];
      updatedFields[fieldIndex] = {
        ...updatedFields[fieldIndex],
        [property]: value
      };
      this.workingFields.set(updatedFields);
      this.hasUnsavedChanges.set(true);
      this.scheduleAutoSave();
    }
  }

  // List view type options
  getListViewTypeOptions(): any[] {
    return [
      { label: 'Text', value: 'text' },
      { label: 'Email', value: 'email' },
      { label: 'Date', value: 'date' },
      { label: 'DateTime', value: 'datetime' },
      { label: 'Number', value: 'number' },
      { label: 'Currency', value: 'currency' },
      { label: 'Percentage', value: 'percentage' },
      { label: 'Boolean', value: 'boolean' },
      { label: 'Badge', value: 'badge' },
      { label: 'Tag', value: 'tag' },
      { label: 'Avatar', value: 'avatar' },
      { label: 'Multiple Avatars', value: 'multiple-avatars' },
      { label: 'Template', value: 'template' },
      { label: 'Link', value: 'link' },
      { label: 'Button', value: 'button' },
      { label: 'Interaction Icon', value: 'interactionIcon' }
    ];
  }

  // Helper methods for template
  getListViewFieldsCount(): number {
    return this.listViewFields().length;
  }

  getAvailableFields(): EntityFieldConfigurationDto[] {
    return this.availableFields();
  }

  getListViewFields(): EntityFieldConfigurationDto[] {
    return this.listViewFields();
  }

  isFieldValid(): boolean {
    const field = this.editingField();
    return field ? !!(field.fieldName && field.dataType) : false;
  }

  // Filtering methods
  onSearchChange(searchTerm: string) {
    this.searchFilter.set(searchTerm);
  }

  onFilterChange() {
    // Filter change is handled by computed filteredFields
  }

  // Field selection methods
  selectField(field: EntityFieldConfigurationDto) {
    this.selectedField.set(field);
  }

  selectFieldForConfig(field: EntityFieldConfigurationDto) {
    this.selectField(field);
  }

  selectFieldAndShowConfig(field: EntityFieldConfigurationDto) {
    this.selectField(field);
  }

  // Dialog methods
  openAddFieldDialog() {
    const newField: EntityFieldConfigurationDto = {
      id: undefined,
      fieldName: '',
      dataType: 'string',
      isRequired: false,
      isActive: true,
      enableChangeLog: false,
      showInListView: false,
      listViewOrder: undefined,
      description: '',
      displayOrder: this.workingFields().length + 1
    };
    this.editingField.set(newField);
    this.showFieldEditDialog.set(true);
  }

  openEditFieldDialog(field: EntityFieldConfigurationDto) {
    this.editingField.set({ ...field });
    this.showFieldEditDialog.set(true);
  }

  closeFieldEditDialog() {
    this.showFieldEditDialog.set(false);
    this.editingField.set(null);
  }

  // Save field changes directly to API
  saveFieldChanges() {
    const field = this.editingField();
    if (!field) return;

    this.fieldSaving.set(true);
    const entityName = this.selectedEntityName();

    // Get current fields and update/add the field
    const allFields = this.workingFields().map((f, index) => ({
      id: f.id,
      fieldName: f.fieldName,
      dataType: f.dataType,
      description: f.description,
      isRequired: f.isRequired,
      isActive: f.isActive,
      enableChangeLog: f.enableChangeLog || false,
      defaultValue: f.defaultValue,
      maxLength: f.maxLength,
      displayOrder: index + 1,
      showInListView: f.showInListView,
      listViewOrder: f.showInListView ? f.listViewOrder : undefined,
      relatedDisplayProperty: f.relatedDisplayProperty,
      displayTemplate: f.displayTemplate,
      listViewLabel: f.listViewLabel,
      listViewType: f.listViewType || 'text',
      listViewWidth: f.listViewWidth,
      listViewEllipsis: f.listViewEllipsis || false,
      listViewSortable: f.listViewSortable !== false,
      firstLetterFallbackField: f.firstLetterFallbackField,
      helperText: f.helperText
    }));

    // Prepare the field for API request
    const fieldRequest = {
      id: field.id,
      fieldName: field.fieldName,
      dataType: field.dataType,
      description: field.description,
      isRequired: field.isRequired,
      isActive: field.isActive,
      enableChangeLog: field.enableChangeLog || false,
      defaultValue: field.defaultValue,
      maxLength: field.maxLength,
      displayOrder: field.displayOrder,
      showInListView: field.showInListView,
      listViewOrder: field.showInListView ? field.listViewOrder : undefined,
      relatedDisplayProperty: field.relatedDisplayProperty,
      displayTemplate: field.displayTemplate,
      listViewLabel: field.listViewLabel,
      listViewType: field.listViewType || 'text',
      listViewWidth: field.listViewWidth,
      listViewEllipsis: field.listViewEllipsis || false,
      listViewSortable: field.listViewSortable !== false,
      firstLetterFallbackField: field.firstLetterFallbackField,
      helperText: field.helperText
    };

    // Find and update existing field or add new one
    if (field.id && field.id > 0) {
      const fieldIndex = allFields.findIndex(f => f.id === field.id);
      if (fieldIndex !== -1) {
        allFields[fieldIndex] = fieldRequest;
      }
    } else {
      // New field - calculate proper list view order if needed
      if (fieldRequest.showInListView) {
        const listViewFields = allFields.filter(f => f.showInListView && f.listViewOrder != null);
        const maxListViewOrder = listViewFields.length > 0 
          ? Math.max(...listViewFields.map(f => f.listViewOrder!))
          : 0;
        fieldRequest.listViewOrder = maxListViewOrder + 1;
      }
      allFields.push(fieldRequest);
    }

    const saveRequest = {
      entityName: entityName,
      description: this.workingEntityConfig().description,
      fields: allFields
    };

    this.entityConfigService.saveEntityConfiguration(entityName, saveRequest).subscribe({
      next: (response: EntityConfigurationDetailsResponse) => {
        // Check if this was a new field being created
        const isNewField = !field.id || field.id <= 0;
        
        if (isNewField && response && response.fields) {
          // Find the newly created field in the response by fieldName
          const savedField = response.fields.find(f => f.fieldName === field.fieldName);
          if (savedField && savedField.id) {
            // Update the local field object with the database-generated ID
            field.id = savedField.id;
            console.log(`Updated new field ${field.fieldName} with ID: ${savedField.id}`);
          }
        }
        
        // Update the current entity config with the complete response
        this.currentEntityConfig.set(response);
        
        // Optimistic update: update local state instead of full reload
        this.updateLocalFieldState(field, fieldRequest);
        
        this.messageService.add({
          severity: 'success',
          summary: 'Success',
          detail: `Field ${field.fieldName} ${isNewField ? 'created' : 'updated'} successfully`
        });
        this.fieldSaving.set(false);
        this.hasUnsavedChanges.set(false);
        this.closeFieldEditDialog();
        
        // Auto-save for new fields: immediately save the configuration again to ensure consistency
        if (isNewField) {
          console.log('Auto-saving configuration after creating new field...');
          this.scheduleAutoSave();
        }
        
        // Only reload sample data if template fields were changed
        if (field.listViewType === 'template' || field.displayTemplate) {
          this.loadSampleData();
        }
      },
      error: (error) => {
        console.error('Error saving field:', error);
        this.fieldSaving.set(false);
        this.messageService.add({
          severity: 'error',
          summary: 'Error',
          detail: 'Failed to save field changes'
        });
      }
    });
  }

  // Helper method to update local field state optimistically
  private updateLocalFieldState(editedField: EntityFieldConfigurationDto, fieldRequest: any) {
    const fields = [...this.workingFields()];
    
    if (editedField.id && editedField.id > 0) {
      // For existing fields, find by ID and update
      const fieldIndex = fields.findIndex(f => f.id === editedField.id);
      if (fieldIndex !== -1) {
        fields[fieldIndex] = {
          ...fields[fieldIndex],
          ...fieldRequest,
          id: editedField.id // Ensure ID is preserved
        };
      } else {
        // This might be a new field that just got an ID - find by fieldName and update
        const fieldByNameIndex = fields.findIndex(f => f.fieldName === editedField.fieldName);
        if (fieldByNameIndex !== -1) {
          fields[fieldByNameIndex] = {
            ...fields[fieldByNameIndex],
            ...fieldRequest,
            id: editedField.id // Update with the new real ID
          };
        } else {
          // Truly new field - add it with the real ID
          const newField = {
            ...fieldRequest,
            id: editedField.id // Use the real ID from database
          };
          fields.push(newField);
        }
      }
    } else {
      // Add new field with temporary ID (this should be rare now)
      const newField = {
        ...fieldRequest,
        id: Date.now() // Temporary ID until next full reload
      };
      fields.push(newField);
    }
    
    this.workingFields.set(fields);
    
    // Update current entity config if available
    const currentConfig = this.currentEntityConfig();
    if (currentConfig) {
      const updatedConfig = {
        ...currentConfig,
        fields: fields
      };
      this.currentEntityConfig.set(updatedConfig);
    }
  }

  // Template preview methods
  private loadSampleData() {
    const entityName = this.selectedEntityName();
    if (entityName) {
      this.entityConfigService.getSampleData(entityName).subscribe({
        next: (data) => {
          this.sampleData.set(data);
          this.updateTemplateAvailableFields(); // Update available fields for autocompletion
        },
        error: (error) => {
          console.warn('Could not load sample data from API:', error);
          this.sampleData.set(null);
          this.templateAvailableFields.set([]); // Clear available fields on error
        }
      });
    }
  }

  // Extract available fields from sample data for autocompletion
  private extractFieldsFromSampleData(obj: any, prefix: string = ''): string[] {
    const fields: string[] = [];
    
    if (!obj || typeof obj !== 'object') {
      return fields;
    }

    for (const key in obj) {
      if (obj.hasOwnProperty(key)) {
        const fieldName = prefix ? `${prefix}.${key}` : key;
        const value = obj[key];
        
        // Add the current field
        fields.push(fieldName);
        
        // If it's an object (but not null, Array, or Date), explore recursively
        if (value && 
            typeof value === 'object' && 
            !Array.isArray(value) && 
            !(value instanceof Date) && 
            Object.keys(value).length > 0) {
          
          // Limit depth to avoid circular references
          if (prefix.split('.').length < 3) {
            fields.push(...this.extractFieldsFromSampleData(value, fieldName));
          }
        }
      }
    }
    
    return fields;
  }

  // Update available fields when sample data changes
  private updateTemplateAvailableFields() {
    const sample = this.sampleData();
    if (sample) {
      const fields = this.extractFieldsFromSampleData(sample);
      this.templateAvailableFields.set(fields.sort());
    } else {
      this.templateAvailableFields.set([]);
    }
  }

  // Auto-save methods
  private scheduleAutoSave(): void {
    if (this.autoSaveTimer) {
      clearTimeout(this.autoSaveTimer);
    }
    
    this.autoSaveTimer = setTimeout(() => {
      if (this.hasUnsavedChanges() && !this.saving() && !this.fieldSaving() && !this.autoSaving()) {
        this.saveAllFields();
      }
    }, 3000); // 3 seconds debounce
  }

  private saveAllFields(): void {
    const entityName = this.selectedEntityName();
    if (!entityName || this.saving() || this.fieldSaving() || this.autoSaving()) return;

    this.autoSaving.set(true);

    const allFields = this.workingFields().map((f, index) => ({
      id: f.id,
      fieldName: f.fieldName,
      dataType: f.dataType,
      description: f.description,
      isRequired: f.isRequired,
      isActive: f.isActive,
      enableChangeLog: f.enableChangeLog || false,
      defaultValue: f.defaultValue,
      maxLength: f.maxLength,
      displayOrder: index + 1,
      showInListView: f.showInListView,
      listViewOrder: f.showInListView ? f.listViewOrder : undefined,
      relatedDisplayProperty: f.relatedDisplayProperty,
      displayTemplate: f.displayTemplate,
      listViewLabel: f.listViewLabel,
      listViewType: f.listViewType || 'text',
      listViewWidth: f.listViewWidth,
      listViewEllipsis: f.listViewEllipsis || false,
      listViewSortable: f.listViewSortable !== false,
      firstLetterFallbackField: f.firstLetterFallbackField,
      helperText: f.helperText
    }));

    const saveRequest = {
      entityName: entityName,
      description: this.workingEntityConfig().description,
      fields: allFields
    };

    this.entityConfigService.saveEntityConfiguration(entityName, saveRequest).subscribe({
      next: () => {
        this.hasUnsavedChanges.set(false);
        this.autoSaving.set(false);
        
        // No need to reload - optimistic update already done
        // Only show success for manual saves, not auto-saves
      },
      error: (error) => {
        console.error('Auto-save failed:', error);
        this.autoSaving.set(false);
        this.messageService.add({
          severity: 'warn',
          summary: 'Auto-save Failed',
          detail: 'Changes could not be saved automatically. Please save manually.',
          life: 5000
        });
      }
    });
  }

  // IDE-style autocompletion methods
  onTemplateInputKeydown(event: KeyboardEvent, inputElement: any) {
    if (!this.showTemplateSuggestions()) {
      // Show suggestions on Ctrl+Space
      if (event.ctrlKey && event.code === 'Space') {
        event.preventDefault();
        this.showAllSuggestions();
        return;
      }
      return;
    }

    const filteredFields = this.templateFilteredFields();
    const currentIndex = this.selectedSuggestionIndex();

    switch (event.key) {
      case 'ArrowDown':
        event.preventDefault();
        if (currentIndex < filteredFields.length - 1) {
          this.selectedSuggestionIndex.set(currentIndex + 1);
        }
        break;
      case 'ArrowUp':
        event.preventDefault();
        if (currentIndex > 0) {
          this.selectedSuggestionIndex.set(currentIndex - 1);
        }
        break;
      case 'Enter':
        event.preventDefault();
        if (filteredFields[currentIndex]) {
          this.selectSuggestion(filteredFields[currentIndex], inputElement);
        }
        break;
      case 'Escape':
        event.preventDefault();
        this.hideTemplateSuggestions();
        break;
    }
  }

  onTemplateInputChange(event: any, inputElement: any) {
    const value = event.target.value;
    const cursorPosition = inputElement.selectionStart || 0;
    
    // Check if user just typed '{'
    if (value[cursorPosition - 1] === '{') {
      this.showAllSuggestions();
    } else if (this.showTemplateSuggestions()) {
      // Filter suggestions based on current word
      const currentWord = this.getCurrentWord(value, cursorPosition);
      this.filterSuggestions(currentWord);
    }
  }

  private getCurrentWord(text: string, cursorPosition: number): string {
    // Find the word being typed after the last '{'
    const beforeCursor = text.substring(0, cursorPosition);
    const lastBraceIndex = beforeCursor.lastIndexOf('{');
    
    if (lastBraceIndex === -1) return '';
    
    const wordStart = lastBraceIndex + 1;
    const currentWord = beforeCursor.substring(wordStart);
    
    // Only return the word if we're still inside braces (no closing '}' found)
    const afterBrace = text.substring(lastBraceIndex);
    const closingBraceIndex = afterBrace.indexOf('}');
    
    if (closingBraceIndex !== -1 && closingBraceIndex < cursorPosition - lastBraceIndex) {
      return '';
    }
    
    return currentWord;
  }

  private showAllSuggestions() {
    this.templateFilteredFields.set(this.templateAvailableFields());
    this.selectedSuggestionIndex.set(0);
    this.showTemplateSuggestions.set(true);
  }

  private filterSuggestions(query: string) {
    const fields = this.templateAvailableFields();
    const filtered = fields.filter(field => 
      field.toLowerCase().includes(query.toLowerCase())
    );
    this.templateFilteredFields.set(filtered);
    this.selectedSuggestionIndex.set(0);
    
    if (filtered.length === 0) {
      this.showTemplateSuggestions.set(false);
    }
  }

  hideTemplateSuggestions() {
    // Use setTimeout to allow click events to fire before hiding
    setTimeout(() => {
      this.showTemplateSuggestions.set(false);
    }, 150);
  }

  selectSuggestion(field: string, inputElement: any) {
    const currentValue = this.editingField()?.displayTemplate || '';
    const cursorPosition = inputElement.selectionStart || 0;
    
    // Find the position where we should insert the field
    const beforeCursor = currentValue.substring(0, cursorPosition);
    const lastBraceIndex = beforeCursor.lastIndexOf('{');
    
    let newValue: string;
    let newCursorPosition: number;
    
    if (lastBraceIndex !== -1) {
      // Replace the partial field name
      const beforeBrace = currentValue.substring(0, lastBraceIndex);
      const afterCursor = currentValue.substring(cursorPosition);
      newValue = beforeBrace + `{${field}}` + afterCursor;
      newCursorPosition = beforeBrace.length + field.length + 2; // +2 for {}
    } else {
      // Insert at cursor position
      const beforeCursor = currentValue.substring(0, cursorPosition);
      const afterCursor = currentValue.substring(cursorPosition);
      newValue = beforeCursor + `{${field}}` + afterCursor;
      newCursorPosition = cursorPosition + field.length + 2; // +2 for {}
    }
    
    // Update the model
    const editingField = this.editingField();
    if (editingField) {
      editingField.displayTemplate = newValue;
      this.onTemplatePatternChange(editingField.id, newValue);
    }
    
    // Hide suggestions and reset focus
    this.showTemplateSuggestions.set(false);
    
    setTimeout(() => {
      inputElement.focus();
      inputElement.setSelectionRange(newCursorPosition, newCursorPosition);
    }, 10);
  }

  // Get field type from sample data for display
  getFieldTypeFromSampleData(fieldPath: string): string {
    const sample = this.sampleData();
    if (!sample) return 'unknown';
    
    try {
      const value = this.getNestedProperty(sample, fieldPath);
      if (value === null || value === undefined) return 'null';
      if (typeof value === 'string') return 'string';
      if (typeof value === 'number') return 'number';
      if (typeof value === 'boolean') return 'boolean';
      if (value instanceof Date) return 'date';
      if (Array.isArray(value)) return 'array';
      if (typeof value === 'object') return 'object';
      return typeof value;
    } catch {
      return 'unknown';
    }
  }

  private createTemplateFunction(templatePattern: string): (rowData: any) => string {
    return (rowData: any) => {
      if (!templatePattern) return '';
      
      return templatePattern.replace(/\{([^}]+)\}/g, (match, expression) => {
        try {
          const value = this.getNestedProperty(rowData, expression.trim());
          return value !== null && value !== undefined ? String(value) : '';
        } catch (error) {
          return '';
        }
      });
    };
  }

  private getNestedProperty(obj: any, path: string): any {
    if (!obj || !path) return null;
    
    return path.split('.').reduce((current, prop) => {
      return current && current[prop] !== undefined ? current[prop] : null;
    }, obj);
  }

  getTemplatePreview(templatePattern: string | undefined): string {
    if (!templatePattern || templatePattern.trim() === '') {
      return '';
    }

    const sample = this.sampleData();
    if (!sample) {
      return this.translateService.instant('entityManager.noDataAvailable');
    }

    try {
      const templateFn = this.createTemplateFunction(templatePattern);
      const result = templateFn(sample);
      return result || this.translateService.instant('entityManager.templateValid');
    } catch (error) {
      console.error('Template preview error:', error);
      return this.translateService.instant('entityManager.templateError');
    }
  }

  deleteField(field: EntityFieldConfigurationDto) {
    this.confirmationService.confirm({
      message: `Are you sure you want to delete the field "${field.fieldName}"?`,
      header: 'Confirm Delete',
      icon: 'pi pi-exclamation-triangle',
      accept: () => {
        const fields = this.workingFields();
        const updatedFields = fields.filter(f => f.id !== field.id);
        
        // Recalculate display orders after deletion
        updatedFields.forEach((field, index) => {
          field.displayOrder = index + 1;
        });
        
        // Recalculate list view orders for remaining fields
        const listViewFields = updatedFields
          .filter(f => f.showInListView)
          .sort((a, b) => (a.listViewOrder ?? 0) - (b.listViewOrder ?? 0));
        
        listViewFields.forEach((field, index) => {
          field.listViewOrder = index + 1;
        });
        
        this.workingFields.set(updatedFields);
        this.hasUnsavedChanges.set(true);
        this.scheduleAutoSave();
        
        this.messageService.add({
          severity: 'success',
          summary: 'Success',
          detail: 'Field deleted successfully'
        });
      }
    });
  }

  // Entity Configuration Dialog Methods
  openEntityConfigurationDialog() {
    const currentConfig = this.workingEntityConfig();
    if (currentConfig) {
      this.entityConfigForm.set({
        id: currentConfig.id,
        entityName: currentConfig.entityName,
        tableName: currentConfig.tableName,
        description: currentConfig.description,
        isActive: currentConfig.isActive,
        enableChangeLog: currentConfig.enableChangeLog
      });
      this.showEntityConfigDialog.set(true);
    }
  }

  closeEntityConfigurationDialog() {
    this.showEntityConfigDialog.set(false);
    this.entityConfigForm.set(null);
  }

  isEntityConfigValid(): boolean {
    const form = this.entityConfigForm();
    if (!form) return false;
    
    return !!(form.entityName?.trim() && form.tableName?.trim());
  }

  saveEntityConfiguration() {
    const form = this.entityConfigForm();
    if (!form || !this.isEntityConfigValid()) return;

    this.configSaving.set(true);

    this.entityConfigService.updateEntityConfiguration(form.id, form).subscribe({
      next: (response) => {
        // Update the working config
        this.workingEntityConfig.set(form);
        
        // Update the current entity config
        const currentConfig = this.currentEntityConfig();
        if (currentConfig) {
          const updatedConfig = {
            ...currentConfig,
            tableName: form.tableName,
            description: form.description,
            isActive: form.isActive,
            enableChangeLog: form.enableChangeLog
          };
          this.currentEntityConfig.set(updatedConfig);
        }

        this.configSaving.set(false);
        this.showEntityConfigDialog.set(false);
        this.entityConfigForm.set(null);
        
        this.messageService.add({
          severity: 'success',
          summary: 'Success',
          detail: 'Entity configuration updated successfully'
        });
      },
      error: (error) => {
        console.error('Error updating entity configuration:', error);
        this.configSaving.set(false);
        this.messageService.add({
          severity: 'error',
          summary: 'Error',
          detail: 'Failed to update entity configuration'
        });
      }
    });
  }

  // Helper method to convert EntityFieldConfigurationDto to ListViewColumn
  private convertFieldToColumn(field: EntityFieldConfigurationDto): ListViewColumn {
    let columnType: ListViewColumn['type'] = 'text';
    
    // Map listViewType to column type
    switch (field.listViewType) {
      case 'avatar':
        columnType = 'avatar';
        break;
      case 'template':
        columnType = 'template';
        break;
      case 'multiple-avatars':
        columnType = 'multiple-avatars';
        break;
      case 'interactionIcon':
        columnType = 'interactionIcon';
        break;
      default:
        // Map based on data type
        columnType = this.getColumnTypeFromDataType(field.dataType);
        break;
    }

    const column: ListViewColumn = {
      label: field.listViewLabel || field.fieldName,
      field: field.fieldName.toLowerCase(), // Normalize to lowercase for consistent data access
      type: columnType,
      sortable: field.listViewSortable || false,
      ellipsis: true, // Enable ellipsis for better card display
    };

    // Add template function for any field that has displayTemplate defined
    if (field.displayTemplate && field.displayTemplate.trim() !== '') {
      column.templateFn = this.createTemplateFunction(field.displayTemplate);
      // Override type to template when displayTemplate is used
      column.type = 'template';
    }

    // Add firstLetterFallbackField for multiple-avatars
    if (field.listViewType === 'multiple-avatars' && field.firstLetterFallbackField) {
      column.firstLetterFallbackField = field.firstLetterFallbackField;
    }

    // Add interaction icon function if it's an interactionIcon type
    if (field.listViewType === 'interactionIcon') {
      const iconData = this.createInteractionIconFunction(field.fieldName);
      column.iconClassFn = (rowData: any) => iconData(rowData).icon;
      column.iconColorFn = (rowData: any) => iconData(rowData).color;
    }

    // Add specific avatar configuration
    if (field.listViewType === 'avatar') {
      // For avatar fields, ensure the firstLetterFallbackField is set if not already configured
      if (field.firstLetterFallbackField) {
        column.firstLetterFallbackField = field.firstLetterFallbackField;
      }
      // Enable ellipsis specifically for avatar columns to handle long names
      column.ellipsis = true;
    }

    return column;
  }

  // Helper method to create interaction icon function
  private createInteractionIconFunction(fieldName: string): (rowData: any) => { icon: string; color: string } {
    return (rowData: any) => {
      const fieldValue = this.getNestedProperty(rowData, fieldName);
      const interactionType = fieldValue || 'default';
      
      return {
        icon: this.interactionIconService.getInteractionMaterialIcon(interactionType),
        color: this.interactionIconService.getInteractionColor(interactionType)
      };
    };
  }

  // Helper method to get column type from data type
  private getColumnTypeFromDataType(dataType: string): ListViewColumn['type'] {
    switch (dataType) {
      case 'datetime':
      case 'date':
        return 'date';
      case 'int':
      case 'number':
        return 'number';
      case 'boolean':
        return 'text'; // Could be enhanced to show as badge
      default:
        return 'text';
    }
  }

  /**
   * @uiButton export_entity_configuration_sql
   * @description Exports all entity configurations as a single SQL script file
   * @label Export Entity Configuration (SQL Script)
   * @icon pi pi-download
   * @when_to_use When you need to export entity configurations as SQL script for database seeding or backup purposes
   * @permissions ENTITY_MANAGER_READ
   */
  exportEntityConfigurationAsSql(): void {
    // Check read permissions
    const permissions = this.permissions();
    if (!permissions.canRead) {
      this.messageService.add({
        severity: 'warn',
        summary: 'Permission Denied',
        detail: 'You do not have permission to export entity configurations'
      });
      return;
    }

    this.saving.set(true);

    const sub = this.entityConfigService.exportEntityConfigurationAsSql().subscribe({
      next: (blob) => {
        console.log('Received blob:', blob);
        console.log('Blob size:', blob.size);
        console.log('Blob type:', blob.type);

        if (blob.size === 0) {
          this.messageService.add({
            severity: 'warn',
            summary: 'Empty Export',
            detail: 'The exported SQL file is empty. Please check if there are entity configurations to export.'
          });
          return;
        }

        // Create download link
        const url = window.URL.createObjectURL(blob);
        const link = document.createElement('a');
        link.href = url;

        // Generate filename with timestamp
        const timestamp = new Date().toISOString().replace(/[:.]/g, '-').slice(0, 19);
        link.download = `EntityConfiguration_${timestamp}.sql`;

        // Trigger download
        document.body.appendChild(link);
        link.click();

        // Cleanup
        document.body.removeChild(link);
        window.URL.revokeObjectURL(url);

        this.messageService.add({
          severity: 'success',
          summary: 'Export Complete',
          detail: 'Entity configurations exported successfully as SQL script file'
        });
      },
      error: (error) => {
        console.error('Error exporting entity configurations as SQL:', error);
        this.messageService.add({
          severity: 'error',
          summary: 'Export Failed',
          detail: 'Failed to export entity configurations as SQL'
        });
      },
      complete: () => {
        this.saving.set(false);
      }
    });

    // Note: We don't need to store this subscription since the component already handles cleanup
  }
}