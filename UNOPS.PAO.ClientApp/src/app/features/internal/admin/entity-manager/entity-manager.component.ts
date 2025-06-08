import { Component, OnInit, signal, computed, inject, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { TranslateModule } from '@ngx-translate/core';
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

// CDK Drag & Drop
import { CdkDragDrop, moveItemInArray, DragDropModule } from '@angular/cdk/drag-drop';

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
    ToggleButtonModule
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

  // State signals
  entities = signal<EntityDropdownModel[]>([]);
  selectedEntityName = signal<string>('');
  currentEntityConfig = signal<EntityConfigurationDetailsResponse | null>(null);
  originalEntityConfig = signal<EntityConfigurationDetailsResponse | null>(null);
  
  // Loading states
  entitiesLoading = signal<boolean>(false);
  configLoading = signal<boolean>(false);
  saving = signal<boolean>(false);
  permissionsLoading = signal<boolean>(true);

  // Additional loading state
  loading = computed(() => this.entitiesLoading() || this.configLoading());

  // Permissions
  permissions = signal<EntityPermissionsModel>({
    entity: 'EntityManager',
    canCreate: false,
    canRead: false,
    canUpdate: false,
    canDelete: false
  });

  // UI state
  hasUnsavedChanges = signal<boolean>(false);
  editingFieldId = signal<number | undefined | null>(null);
  showListViewPanel = signal<boolean>(false);
  showFieldEditPanel = signal<boolean>(false);

  // Field editing state
  editingField = signal<EntityFieldConfigurationDto | null>(null);

  // Dialog states
  showFieldEditDialog = signal<boolean>(false);
  showListViewDialog = signal<boolean>(false);

  // Temporary state for list view management (doesn't affect main working fields until saved)
  tempListViewFields = signal<EntityFieldConfigurationDto[]>([]);
  tempAvailableFields = signal<EntityFieldConfigurationDto[]>([]);
  listViewHasChanges = signal<boolean>(false);

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

  // Computed values
  hasAccessToManage = computed(() => this.permissions().canUpdate);
  canViewOnly = computed(() => this.permissions().canRead && !this.permissions().canUpdate);
  entityOptions = computed(() => 
    this.entities().map(entity => ({ 
      label: entity.entityName, 
      value: entity.entityName 
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
    entityName: '',
    tableName: '',
    description: '',
    isActive: true
  });

  workingFields = signal<EntityFieldConfigurationDto[]>([]);

  ngOnInit() {
    this.loadPermissions();
    this.resetDialogStates();
    setTimeout(() => this.cdr.detectChanges(), 0);
  }

  private resetDialogStates() {
    this.editingFieldId.set(null);
    this.editingField.set(null);
    this.showListViewPanel.set(false);
    this.showFieldEditPanel.set(false);
    this.showFieldEditDialog.set(false);
    this.showListViewDialog.set(false);
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
        this.cdr.detectChanges();
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
        this.entities.set(entities);
        this.entitiesLoading.set(false);
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
      return;
    }

    this.relatedFieldsCache.clear();
    this.loadEntityConfiguration(entityName);
  }

  private loadEntityConfiguration(entityName: string) {
    this.configLoading.set(true);
    this.entityConfigService.getEntityConfiguration(entityName).subscribe({
      next: (config) => {
        this.currentEntityConfig.set(config);
        this.originalEntityConfig.set(JSON.parse(JSON.stringify(config)));
        
        this.workingEntityConfig.set({
          entityName: config.entityName,
          tableName: config.tableName,
          description: config.description || '',
          isActive: config.isActive
        });
        
        const sortedFields = [...config.fields].sort((a, b) => a.displayOrder - b.displayOrder);
        this.workingFields.set(sortedFields);
        
        this.configLoading.set(false);
        this.hasUnsavedChanges.set(false);
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
        const maxListViewOrder = currentListViewFields.length > 0
          ? Math.max(...currentListViewFields.map(f => f.listViewOrder!))
          : 0;
        
        updatedFields[fieldIndex] = {
          ...updatedFields[fieldIndex],
          showInListView: true,
          listViewOrder: maxListViewOrder + 1
        };
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
      }
      
      this.workingFields.set(updatedFields);
      this.hasUnsavedChanges.set(true);
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

  // Get available display properties for a related entity type
  getRelatedEntityFields(entityType: string): Observable<RelatedFieldOption[]> {
    return this.entityConfigService.getRelatedEntityFields(entityType);
  }

  // Helper method to convert field names to proper camelCase
  private toCamelCase(fieldName: string): string {
    if (fieldName.includes('.')) {
      const parts = fieldName.split('.');
      return parts.map(part => this.toCamelCase(part)).join('.');
    }
    
    const fieldMappings: { [key: string]: string } = {
      'shortname': 'shortName',
      'firstname': 'firstName',
      'lastname': 'lastName',
      'middlename': 'middleName',
      'fullname': 'fullName',
      'partnercode': 'partnerCode',
      'organizationname': 'organizationName',
      'businessunit': 'businessUnit',
      'contactperson': 'contactPerson',
      'phonenumber': 'phoneNumber',
      'mobilenumber': 'mobileNumber',
      'emailaddress': 'emailAddress',
      'postalcode': 'postalCode',
      'createdate': 'createDate',
      'updatedate': 'updateDate',
      'createdby': 'createdBy',
      'updatedby': 'updatedBy',
      'isactive': 'isActive',
      'isdeleted': 'isDeleted'
    };
    
    const lowerFieldName = fieldName.toLowerCase();
    
    if (fieldMappings[lowerFieldName]) {
      return fieldMappings[lowerFieldName];
    }
    
    return fieldName
      .toLowerCase()
      .replace(/[_-](.)/g, (_, char) => char.toUpperCase())
      .replace(/^(.)/, (match) => match.toLowerCase());
  }

  // Helper method to get dropdown options for display field path (for ALL field types)
  getRelatedDisplayOptions(dataType: string): Observable<any[]> {
    if (this.isRelationshipField(dataType)) {
      const baseEntityType = dataType.replace('[]', '');
      return this.getEntityFieldOptions(baseEntityType);
    }
    
    const currentEntityName = this.selectedEntityName();
    if (currentEntityName) {
      return this.getEntityFieldOptions(currentEntityName, true);
    }
    
    return of([]);
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
          ? this.toCamelCase(opt.value)
          : opt.fieldPath || `${entityType.toLowerCase()}.${this.toCamelCase(opt.value)}`;
        
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

  // Handle display field path changes from dropdown
  onDisplayFieldPathChange(fieldId: number | undefined, value: string) {
    const fields = this.workingFields();
    const fieldIndex = fields.findIndex(f => f.id === fieldId);
    if (fieldIndex !== -1) {
      const updatedFields = [...fields];
      updatedFields[fieldIndex] = {
        ...updatedFields[fieldIndex],
        displayFieldPath: value
      };
      this.workingFields.set(updatedFields);
      this.hasUnsavedChanges.set(true);
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
          
          const fieldPath = isSameEntity 
            ? this.toCamelCase(value)
            : selectedOption?.fieldPath || `${baseEntityType.toLowerCase()}.${this.toCamelCase(value)}`;
          
          updatedFields[fieldIndex] = {
            ...updatedFields[fieldIndex],
            relatedDisplayProperty: value,
            displayFieldPath: fieldPath,
            displayTemplate: selectedOption?.isTemplate ? selectedOption.templatePattern : undefined,
            listViewType: selectedOption?.isTemplate ? 'template' : 'text'
          };
          
          this.workingFields.set(updatedFields);
          this.hasUnsavedChanges.set(true);
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
      { label: 'Button', value: 'button' }
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

  // Helper methods for temporary list view data (used in dialog)
  getTempListViewFields(): EntityFieldConfigurationDto[] {
    return this.tempListViewFields();
  }

  getTempAvailableFields(): EntityFieldConfigurationDto[] {
    return this.tempAvailableFields();
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

  openListViewManagementDialog() {
    // Initialize temporary state with current field states
    const allFields = this.workingFields();
    const currentListViewFields = allFields
      .filter(field => field.showInListView && field.isActive)
      .sort((a, b) => (a.listViewOrder ?? 0) - (b.listViewOrder ?? 0));
    const currentAvailableFields = allFields.filter(field => !field.showInListView && field.isActive);
    
    this.tempListViewFields.set([...currentListViewFields]);
    this.tempAvailableFields.set([...currentAvailableFields]);
    this.listViewHasChanges.set(false);
    this.showListViewDialog.set(true);
  }

  closeListViewDialog() {
    this.showListViewDialog.set(false);
    this.listViewHasChanges.set(false);
  }

  // Temporary list view field management (doesn't save until saveListViewChanges is called)
  tempMoveFieldToListView(fieldId: number | undefined) {
    const availableFields = this.tempAvailableFields();
    const listViewFields = this.tempListViewFields();
    
    const fieldIndex = availableFields.findIndex(f => f.id === fieldId);
    if (fieldIndex !== -1) {
      const field = availableFields[fieldIndex];
      const updatedAvailable = availableFields.filter(f => f.id !== fieldId);
      
      // Calculate new list view order
      const maxListViewOrder = listViewFields.length > 0
        ? Math.max(...listViewFields.map(f => f.listViewOrder || 0))
        : 0;
      
      const updatedField = {
        ...field,
        showInListView: true,
        listViewOrder: maxListViewOrder + 1
      };
      
      this.tempAvailableFields.set(updatedAvailable);
      this.tempListViewFields.set([...listViewFields, updatedField]);
      this.listViewHasChanges.set(true);
    }
  }

  tempRemoveFieldFromListView(fieldId: number | undefined) {
    const listViewFields = this.tempListViewFields();
    const availableFields = this.tempAvailableFields();
    
    const fieldIndex = listViewFields.findIndex(f => f.id === fieldId);
    if (fieldIndex !== -1) {
      const field = listViewFields[fieldIndex];
      const updatedListView = listViewFields.filter(f => f.id !== fieldId);
      
      // Recalculate list view orders for remaining fields
      updatedListView.forEach((f, index) => {
        f.listViewOrder = index + 1;
      });
      
      const updatedField = {
        ...field,
        showInListView: false,
        listViewOrder: undefined
      };
      
      this.tempListViewFields.set(updatedListView);
      this.tempAvailableFields.set([...availableFields, updatedField]);
      this.listViewHasChanges.set(true);
    }
  }

  tempOnListViewFieldDrop(event: CdkDragDrop<EntityFieldConfigurationDto[]>) {
    const listViewFields = [...this.tempListViewFields()];
    moveItemInArray(listViewFields, event.previousIndex, event.currentIndex);
    
    // Update list view order based on new positions
    listViewFields.forEach((field, index) => {
      field.listViewOrder = index + 1;
    });
    
    this.tempListViewFields.set(listViewFields);
    this.listViewHasChanges.set(true);
  }

  // Save list view changes to working fields
  saveListViewChanges() {
    const allFields = [...this.workingFields()];
    const tempListView = this.tempListViewFields();
    const tempAvailable = this.tempAvailableFields();
    
    // Update all affected fields
    [...tempListView, ...tempAvailable].forEach(tempField => {
      const fieldIndex = allFields.findIndex(f => f.id === tempField.id);
      if (fieldIndex !== -1) {
        allFields[fieldIndex] = {
          ...allFields[fieldIndex],
          showInListView: tempField.showInListView,
          listViewOrder: tempField.listViewOrder
        };
      }
    });
    
    this.workingFields.set(allFields);
    this.hasUnsavedChanges.set(true);
    this.listViewHasChanges.set(false);
    
    this.messageService.add({
      severity: 'success',
      summary: 'Success',
      detail: 'List view configuration saved successfully'
    });
    
    this.closeListViewDialog();
  }

  // Save field changes directly to API
  saveFieldChanges() {
    const field = this.editingField();
    if (!field) return;

    this.saving.set(true);
    const entityName = this.selectedEntityName();

    // Get current fields and update/add the field
    const allFields = this.workingFields().map((f, index) => ({
      id: f.id,
      fieldName: f.fieldName,
      dataType: f.dataType,
      description: f.description,
      isRequired: f.isRequired,
      isActive: f.isActive,
      defaultValue: f.defaultValue,
      maxLength: f.maxLength,
      displayOrder: index + 1,
      showInListView: f.showInListView,
      listViewOrder: f.showInListView ? f.listViewOrder : undefined,
      relatedDisplayProperty: f.relatedDisplayProperty,
      displayFieldPath: f.displayFieldPath,
      displayTemplate: f.displayTemplate,
      listViewLabel: f.listViewLabel,
      listViewType: f.listViewType || 'text',
      listViewWidth: f.listViewWidth,
      listViewEllipsis: f.listViewEllipsis || false,
      listViewSortable: f.listViewSortable !== false,
      firstLetterFallbackField: f.firstLetterFallbackField
    }));

    // Prepare the field for API request
    const fieldRequest = {
      id: field.id,
      fieldName: field.fieldName,
      dataType: field.dataType,
      description: field.description,
      isRequired: field.isRequired,
      isActive: field.isActive,
      defaultValue: field.defaultValue,
      maxLength: field.maxLength,
      displayOrder: field.displayOrder,
      showInListView: field.showInListView,
      listViewOrder: field.showInListView ? field.listViewOrder : undefined,
      relatedDisplayProperty: field.relatedDisplayProperty,
      displayFieldPath: field.displayFieldPath,
      displayTemplate: field.displayTemplate,
      listViewLabel: field.listViewLabel,
      listViewType: field.listViewType || 'text',
      listViewWidth: field.listViewWidth,
      listViewEllipsis: field.listViewEllipsis || false,
      listViewSortable: field.listViewSortable !== false,
      firstLetterFallbackField: field.firstLetterFallbackField
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
      next: () => {
        this.messageService.add({
          severity: 'success',
          summary: 'Success',
          detail: `Field ${field.fieldName} saved successfully`
        });
        this.saving.set(false);
        this.closeFieldEditDialog();
        
        // Reload the configuration to get updated data
        this.loadEntityConfiguration(entityName);
      },
      error: (error) => {
        console.error('Error saving field:', error);
        this.saving.set(false);
        this.messageService.add({
          severity: 'error',
          summary: 'Error',
          detail: 'Failed to save field changes'
        });
      }
    });
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
        
        this.messageService.add({
          severity: 'success',
          summary: 'Success',
          detail: 'Field deleted successfully'
        });
      }
    });
  }
} 