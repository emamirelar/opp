import { Component, OnInit, OnDestroy, ChangeDetectionStrategy, signal, computed, inject, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators, FormsModule, AbstractControl, ValidationErrors } from '@angular/forms';
import { TranslateModule } from '@ngx-translate/core';
import { Subscription } from 'rxjs';
import { Router } from '@angular/router';

// PrimeNG imports
import { TableModule } from 'primeng/table';
import { ButtonModule } from 'primeng/button';
import { DialogModule } from 'primeng/dialog';
import { InputTextModule } from 'primeng/inputtext';
import { TextareaModule } from 'primeng/textarea';
import { DropdownModule } from 'primeng/dropdown';
import { ToastModule } from 'primeng/toast';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { TooltipModule } from 'primeng/tooltip';
import { ProgressSpinnerModule } from 'primeng/progressspinner';
import { IconFieldModule } from 'primeng/iconfield';
import { InputIconModule } from 'primeng/inputicon';
import { SliderModule } from 'primeng/slider';
import { ToggleSwitchModule } from 'primeng/toggleswitch';
import { RadioButtonModule } from 'primeng/radiobutton';
import { MessageService } from 'primeng/api';
import { ConfirmationService } from 'primeng/api';

// Services and models
import { AiPromptService, AiPrompt, AiPromptFilterRequest, GeminiModel, GenerationConfig, ToolsConfig, TestPromptRequest, TestPromptResponse } from '../../services/ai-prompt.service';
import { PaginationResponse } from '../../../../common/models/pagination-response.model';
import { PermissionService, EntityPermissions } from '../../../../essentials/services/permission.service';
import { ConfigurationService } from '../../../../essentials/services/configuration.service';

interface LocalAiPromptFilterRequest {
  pageIndex: number;
  pageSize: number;
  orderBy?: string;
  ascending?: boolean;
  searchText?: string;
}

interface AiPromptListRequest {
  request: LocalAiPromptFilterRequest;
}

interface TestResult {
  success: boolean;
  response?: string;
  error?: string;
}

interface ConfigurationData {
  projectId?: string;
  location?: string;
  defaultModel?: string;
}

// Custom validator for prompt data
function promptDataValidator(control: AbstractControl): ValidationErrors | null {
  const value = control.value;
  if (value && !value.includes('{promptData}')) {
    return { promptDataRequired: true };
  }
  return null;
}

// Custom validator for underscore-separated type field
function underscoreValidator(control: AbstractControl): ValidationErrors | null {
  const value = control.value;
  if (!value) return null;
  
  // Check if the value contains only lowercase letters, numbers, and underscores
  // and doesn't start or end with underscore
  const underscorePattern = /^[a-z0-9]+(_[a-z0-9]+)*$/;
  
  if (!underscorePattern.test(value)) {
    return { underscoreFormat: true };
  }
  return null;
}

@Component({
  selector: 'app-ai-prompt',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    FormsModule,
    TranslateModule,
    TableModule,
    ButtonModule,
    DialogModule,
    InputTextModule,
    TextareaModule,
    DropdownModule,
    ToastModule,
    ConfirmDialogModule,
    TooltipModule,
    ProgressSpinnerModule,
    IconFieldModule,
    InputIconModule,
    SliderModule,
    ToggleSwitchModule,
    RadioButtonModule
  ],
  providers: [MessageService, ConfirmationService],
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: './ai-prompt.component.html',
  styleUrls: ['./ai-prompt.component.scss']
})
export class AiPromptComponent implements OnInit, OnDestroy {
  // Injected services
  private router = inject(Router);
  private permissionService = inject(PermissionService);
  private configurationService = inject(ConfigurationService);
  private cdr = inject(ChangeDetectorRef);

  // Signals for reactive state
  prompts = signal<AiPrompt[]>([]);
  loading = signal(false);
  saving = signal(false);
  testing = signal(false);
  displayDialog = signal(false);
  totalRecords = signal(0);
  pageSize = signal(10);
  searchText = '';
  geminiModels = signal<GeminiModel[]>([]);
  testResults = signal<TestResult | null>(null);
  activeTab = signal<'preview' | 'raw'>('preview');
  configurationData = signal<ConfigurationData>({});
  showCreateBanner = signal(false);
  
  // Table state management for proper pagination, sorting, and search
  private currentTableState: any = {
    first: 0,
    rows: 10,
    sortField: 'type',
    sortOrder: 1,
    filters: {}
  };
  
  // Search debouncing
  private searchTimeout: any = null;
  private readonly SEARCH_DEBOUNCE_TIME = 800; // 800ms delay
  
  // Permission signals
  entityPermissions = signal<EntityPermissions>({
    entity: 'aipromptmanagement',
    hasAccess: false,
    permissions: {
      canRead: false,
      canCreate: false,
      canUpdate: false,
      canDelete: false
    }
  });
  permissionsLoading = signal<boolean>(true);
  
  // Current prompt being edited
  currentPrompt = signal<AiPrompt | null>(null);
  
  // Track prompt function value for reactivity
  promptFunctionValue = signal<string>('');
  
  // Track selected model for reactivity
  selectedModelValue = signal<string>('');
  
  // Computed values
  dialogTitle = computed(() => 
    this.currentPrompt() ? 'Edit AI Prompt' : 'Create AI Prompt'
  );

  isEditMode = computed(() => !!this.currentPrompt());

  // Show "Use Entity ID" option only when promptFunction has a value
  showEntityIdOption = computed(() => {
    const promptFunction = this.promptFunctionValue();
    return !!(promptFunction && promptFunction.trim().length > 0);
  });

  // Auto-switch to test data mode when function is not available
  shouldUseTestData = computed(() => !this.showEntityIdOption());

  // Get max tokens for the selected model
  selectedModelMaxTokens = computed(() => {
    const modelValue = this.selectedModelValue();
    const models = this.geminiModels();
    if (modelValue && models.length > 0) {
      const selectedModel = models.find(m => m.value === modelValue);
      return selectedModel?.maxTokens || 8192;
    }
    return 8192;
  });

  // Filter out preview models (only include production models)
  productionGeminiModels = computed(() => {
    return this.geminiModels().filter(model => 
      !model.label.toLowerCase().includes('preview')
    );
  });

  // Form
  promptForm: FormGroup;
  
  // Subscriptions
  private subscriptions = new Subscription();

  constructor(
    private aiPromptService: AiPromptService,
    private messageService: MessageService,
    private confirmationService: ConfirmationService,
    private fb: FormBuilder
  ) {
    this.promptForm = this.createForm();
  }

  ngOnInit(): void {
    // Load configuration, permissions and models
    this.loadConfiguration();
    this.loadPermissions();
    this.loadGeminiModels();
  }

  ngOnDestroy(): void {
    this.subscriptions.unsubscribe();
    
    // Clear search timeout to prevent memory leaks
    if (this.searchTimeout) {
      clearTimeout(this.searchTimeout);
      this.searchTimeout = null;
    }
  }

  private loadConfiguration(): void {
    const config = this.configurationService.getConfig();
    if (config) {
      this.configurationData.set({
        projectId: config.projectId,
        location: config.location,
        defaultModel: config.defaultModel
      });
      
      // Recreate form with configuration defaults if form is already created
      if (this.promptForm) {
        this.promptForm = this.createForm();
        this.cdr.detectChanges();
      }
    } else {
      // Set empty configuration data as fallback
      this.configurationData.set({});
    }
  }

  private loadPermissions(): void {
    this.permissionsLoading.set(true);
    
    // Clear cache before loading to ensure fresh permissions
    this.permissionService.clearPermissionCaches();
    
    // Use the correct entity name for AI prompt management (updated to match backend)
    const entityName = 'aipromptmanagement';
    
    // Load from server (cache was cleared above)
    this.permissionService.getEntityPermissions(entityName)
      .subscribe({
        next: (permissions) => {
          if (!permissions.hasAccess) {
            
            this.router.navigate(['/access-denied']);
            return;
          }
          
          this.entityPermissions.set(permissions);
          this.permissionsLoading.set(false);
          this.cdr.detectChanges();
          
          // Load data after permissions are confirmed
          if (permissions.hasAccess) {
            this.loadPrompts();
          }
        },
        error: (error) => {
          console.error(`Error loading ${entityName} permissions:`, error);
          this.permissionsLoading.set(false);
          this.messageService.add({
            severity: 'error',
            summary: 'Access Error',
            detail: 'Unable to verify permissions for AI prompt management'
          });
          this.cdr.detectChanges();
        }
      });
  }

  private loadGeminiModels(): void {
    const sub = this.aiPromptService.getGeminiModels().subscribe({
      next: (response) => {
        if (response.body) {
          this.geminiModels.set(response.body);
        }
      },
      error: (error) => {
        console.error('Error loading Gemini models:', error);
        this.messageService.add({
          severity: 'error',
          summary: 'Error',
          detail: 'Failed to load Gemini models'
        });
      }
    });

    this.subscriptions.add(sub);
  }

  loadPrompts(): void {
    // Use current table state for consistency
    this.onLazyLoad(this.currentTableState);
  }

  onLazyLoad(event: any): void {
    // Save current table state for search functionality
    this.currentTableState = {
      first: event.first || 0,
      rows: event.rows || 10,
      sortField: event.sortField || 'type',
      sortOrder: event.sortOrder || 1,
      filters: event.filters || {}
    };

    this.loading.set(true);
    
    const requestParams: LocalAiPromptFilterRequest = {
      pageIndex: Math.floor(this.currentTableState.first / this.currentTableState.rows) + 1,
      pageSize: this.currentTableState.rows,
      orderBy: this.currentTableState.sortField,
      // Fix sorting: sortOrder 1 = ascending, -1 = descending
      ascending: this.currentTableState.sortOrder !== -1,
      searchText: this.searchText || undefined
    };

    const requestBody: any = requestParams;

    this.pageSize.set(this.currentTableState.rows);

    const sub = this.aiPromptService.getPrompts(requestBody).subscribe({
      next: (response) => {
        if (response.body) {
          this.prompts.set(response.body.records);
          this.totalRecords.set(response.body.totalCount);
        }
        this.loading.set(false);
      },
      error: (error) => {
        console.error('Error loading prompts:', error);
        this.messageService.add({
          severity: 'error',
          summary: 'Error',
          detail: 'Failed to load AI prompts'
        });
        this.loading.set(false);
      }
    });

    this.subscriptions.add(sub);
  }

  onSearch(): void {
    // Clear existing timeout
    if (this.searchTimeout) {
      clearTimeout(this.searchTimeout);
    }
    
    // Debounce search - wait for user to stop typing
    this.searchTimeout = setTimeout(() => {
      this.performSearch();
    }, this.SEARCH_DEBOUNCE_TIME);
  }

  onSearchEnter(): void {
    // Immediate search on Enter key - clear any pending debounced search
    if (this.searchTimeout) {
      clearTimeout(this.searchTimeout);
      this.searchTimeout = null;
    }
    this.performSearch();
  }

  private performSearch(): void {
    // Reset to first page when searching and trigger lazy load
    this.currentTableState.first = 0;
    this.onLazyLoad(this.currentTableState);
  }

  clearSearch(): void {
    // Clear any pending search
    if (this.searchTimeout) {
      clearTimeout(this.searchTimeout);
      this.searchTimeout = null;
    }
    this.searchText = '';
    this.performSearch();
  }

  openEditDialog(prompt?: AiPrompt): void {
    // Check permissions before opening dialog
    const permissions = this.entityPermissions();
    if (prompt && !permissions.permissions.canUpdate) {
      this.messageService.add({
        severity: 'warn',
        summary: 'Permission Denied',
        detail: 'You do not have permission to edit AI prompts'
      });
      return;
    }
    
    if (!prompt && !permissions.permissions.canCreate) {
      this.messageService.add({
        severity: 'warn',
        summary: 'Permission Denied',
        detail: 'You do not have permission to create AI prompts'
      });
      return;
    }

    this.currentPrompt.set(prompt || null);
    
    // Show banner for new prompt creation
    this.showCreateBanner.set(!prompt);
    
    const defaultContentConfig = JSON.stringify({
      "role": "user",
      "parts": [
        {
          "text": "{promptData}"
        }
      ]
    });
    
    if (prompt) {
      // Parse generation config
      let generationConfig: GenerationConfig = {};
      try {
        generationConfig = JSON.parse(prompt.generationConfig || '{}');
      } catch (e) {
        console.warn('Failed to parse generation config:', e);
      }

      // Parse tools config - handle both old and new formats
      let googleSearchEnabled = false;
      try {
        const toolsConfigString = prompt.toolsConfig || '[]';
        const toolsData = JSON.parse(toolsConfigString);
        
        // New format: array with objects like [{ "googleSearch": {} }]
        if (Array.isArray(toolsData)) {
          googleSearchEnabled = toolsData.some(tool => tool.googleSearch !== undefined);
        } 
        // Old format: object like { "googleSearch": true/false }
        else if (typeof toolsData === 'object' && toolsData !== null) {
          googleSearchEnabled = toolsData.googleSearch || false;
        }
      } catch (e) {
        console.warn('Failed to parse tools config:', e);
        googleSearchEnabled = false;
      }

      this.promptForm.patchValue({
        type: prompt.type,
        promptFunction: prompt.promptFunction,
        prompt: prompt.prompt,
        description: prompt.description,
        project: prompt.project,
        location: prompt.location,
        model: prompt.model,
        temperature: this.ensureNumber(generationConfig.temperature, 1),
        topP: this.ensureNumber(generationConfig.top_p, 0.2),
        maxOutputTokens: this.ensureNumber(generationConfig.max_output_tokens, 8192),
        googleSearch: googleSearchEnabled,
        safetySettings: prompt.safetySettings
      });
      
      // Update the signals for reactivity
      this.promptFunctionValue.set(prompt.promptFunction || '');
      this.selectedModelValue.set(prompt.model || '');
      
      // Disable type field on edit
      this.promptForm.get('type')?.disable();
    } else {
      // Reset form for new prompt
      this.promptForm.reset();
      
      // Get configuration data for defaults
      const config = this.configurationData();
      
      // Set default values for new prompt
      this.promptForm.patchValue({
        contentConfig: defaultContentConfig,
        testMode: 'testData', // Default to test data mode for new prompts
        temperature: 1,
        topP: 0.2,
        maxOutputTokens: 8192,
        googleSearch: false,
        project: config?.projectId || '',
        location: config?.location || '',
        model: config?.defaultModel || ''
      });
      
      // Reset the signal for new prompts
      this.promptFunctionValue.set('');
      this.selectedModelValue.set('');
      
      // Enable type field for new prompt
      this.promptForm.get('type')?.enable();
    }
    
    // Always disable project and location fields (they should never be editable)
    this.promptForm.get('project')?.disable();
    this.promptForm.get('location')?.disable();
    
    // Clear test results and reset active tab
    this.testResults.set(null);
    this.activeTab.set('preview');
    
    this.displayDialog.set(true);
    
    // Trigger change detection for computed signals
    this.cdr.detectChanges();
  }

  private ensureNumber(value: any, defaultValue: number): number {
    if (value === null || value === undefined || isNaN(Number(value))) {
      return defaultValue;
    }
    return Number(value);
  }

  closeDialog(): void {
    this.displayDialog.set(false);
    this.currentPrompt.set(null);
    this.showCreateBanner.set(false);
    
    // Re-enable all form controls before reset
    this.promptForm.get('type')?.enable();
    this.promptForm.get('project')?.enable();
    this.promptForm.get('location')?.enable();
    
    this.promptForm.reset();
    
    // Clear test-related data
    this.testResults.set(null);
    this.activeTab.set('preview');
    
    // Reset the prompt function signal
    this.promptFunctionValue.set('');
    this.selectedModelValue.set('');
    
    // Get configuration data for defaults
    const config = this.configurationData();
    
    // Reset form with default values including test mode
    const defaultContentConfig = JSON.stringify({
      "role": "user",
      "parts": [
        {
          "text": "{promptData}"
        }
      ]
    });
    
    this.promptForm.patchValue({
      type: '',
      promptFunction: '',
      prompt: '',
      description: '',
      contentConfig: defaultContentConfig,
      project: config?.projectId || '',
      location: config?.location || '',
      model: config?.defaultModel || '',
      testMode: 'testData', // Reset to test data mode by default
      entityId: '',
      testData: '',
      temperature: 1,
      topP: 0.2,
      maxOutputTokens: 8192,
      googleSearch: false,
      safetySettings: ''
    });
    
    // Trigger change detection for computed signals
    this.cdr.detectChanges();
  }

  savePrompt(): void {
    if (this.promptForm.invalid) {
      this.markFormGroupTouched(this.promptForm);
      return;
    }

    this.saving.set(true);
    const formValue = this.promptForm.value;
    
    // Get raw values for disabled fields (project and location)
    const projectValue = this.promptForm.get('project')?.value;
    const locationValue = this.promptForm.get('location')?.value;
    const typeValue = this.promptForm.get('type')?.value;

    // Build generation config
    const generationConfig: GenerationConfig = {
      temperature: formValue.temperature,
      top_p: formValue.topP,
      max_output_tokens: formValue.maxOutputTokens
    };

    // Build tools config in the correct format: "[{ "googleSearch": {} }]" when enabled, "[]" when disabled
    let toolsConfigArray: any[] = [];
    if (formValue.googleSearch) {
      toolsConfigArray.push({ "googleSearch": {} });
    }

    const promptData: AiPrompt = {
      id: this.currentPrompt()?.id || 0,
      type: typeValue,
      promptFunction: formValue.promptFunction,
      prompt: formValue.prompt,
      description: formValue.description,
      name: this.currentPrompt()?.name || typeValue, // Preserve original name for existing prompts, use type for new ones
      generationConfig: JSON.stringify(generationConfig),
      contentConfig: formValue.contentConfig,
      toolsConfig: JSON.stringify(toolsConfigArray),
      safetySettings: formValue.safetySettings,
      project: projectValue,
      location: locationValue,
      model: formValue.model,
      createdAt: this.currentPrompt()?.createdAt || new Date()
    };

    const operation = this.currentPrompt() 
      ? this.aiPromptService.updatePrompt(this.currentPrompt()!.id, promptData)
      : this.aiPromptService.createPrompt(promptData);

    const sub = operation.subscribe({
      next: (response) => {
        this.messageService.add({
          severity: 'success',
          summary: 'Success',
          detail: this.currentPrompt() ? 'Prompt updated successfully' : 'Prompt created successfully'
        });
        this.closeDialog();
        this.loadPrompts();
      },
      error: (error) => {
        console.error('Error saving prompt:', error);
        this.messageService.add({
          severity: 'error',
          summary: 'Error',
          detail: 'Failed to save prompt'
        });
      },
      complete: () => {
        this.saving.set(false);
      }
    });

    this.subscriptions.add(sub);
  }

  testPrompt(): void {
    if (!this.canRunTest()) {
      this.messageService.add({
        severity: 'warn',
        summary: 'Warning',
        detail: 'Please fill in all required fields and test data before testing'
      });
      return;
    }

    this.testing.set(true);
    this.testResults.set(null);
    this.activeTab.set('preview'); // Reset to preview tab for new test
    
    const formValue = this.promptForm.value;
    
    const testRequest: TestPromptRequest = {
      type: formValue.type,
      ...(formValue.testMode === 'entityId' 
        ? { id: formValue.entityId }
        : { testData: formValue.testData }
      ),
      // Optional overrides for testing
      prompt: formValue.prompt,
      model: formValue.model,
      project: formValue.project,
      location: formValue.location,
      temperature: formValue.temperature,
      topP: formValue.topP,
      maxOutputTokens: formValue.maxOutputTokens,
      googleSearch: formValue.googleSearch,
      safetySettings: formValue.safetySettings
    };

    const sub = this.aiPromptService.testPrompt(testRequest).subscribe({
      next: (response) => {
        if (response.body) {
          this.testResults.set({
            success: response.body.success,
            response: response.body.response,
            error: response.body.error
          });
        }
      },
      error: (error) => {
        console.error('Error testing prompt:', error);
        this.testResults.set({
          success: false,
          error: error.error?.message || 'Failed to test prompt'
        });
      },
      complete: () => {
        this.testing.set(false);
      }
    });

    this.subscriptions.add(sub);
  }

  canRunTest(): boolean {
    const form = this.promptForm;
    
    // Check all required left-side fields (excluding promptFunction as it's optional)
    const leftSideValid = !!(
      form.get('type')?.valid &&
      form.get('prompt')?.valid &&
      form.get('model')?.valid &&
      form.get('project')?.valid &&
      form.get('location')?.valid
    );
    
    if (!leftSideValid) {
      return false;
    }
    
    // Check test mode specific fields
    const testMode = form.get('testMode')?.value;
    const hasFunction = this.showEntityIdOption();
    
    if (testMode === 'entityId') {
      // Can only test with entity ID if function is provided
      return hasFunction && !!(
        form.get('entityId')?.value &&
        form.get('entityId')?.value > 0
      );
    } else if (testMode === 'testData') {
      return !!(
        form.get('testData')?.value &&
        form.get('testData')?.value.trim().length > 0
      );
    }
    
    return false;
  }

  confirmDelete(prompt: AiPrompt): void {
    // Check delete permissions
    const permissions = this.entityPermissions();
    if (!permissions.permissions.canDelete) {
      this.messageService.add({
        severity: 'warn',
        summary: 'Permission Denied',
        detail: 'You do not have permission to delete AI prompts'
      });
      return;
    }

    const featureName = prompt.name || 'Unknown';
    
    this.confirmationService.confirm({
      message: `Are you sure you want to delete the AI prompt "${prompt.type}"? 

⚠️ This prompt is currently being used by the ${featureName} feature. Deleting it will affect the functionality of this feature.

Be extra cautious while deleting as there could be several dependencies within the application.`,
      header: 'Confirm Delete - Feature Impact Warning',
      icon: 'pi pi-exclamation-triangle',
      accept: () => {
        this.deletePrompt(prompt);
      }
    });
  }

  private deletePrompt(prompt: AiPrompt): void {
    const sub = this.aiPromptService.deletePrompt(prompt.id).subscribe({
      next: () => {
        this.messageService.add({
          severity: 'success',
          summary: 'Success',
          detail: 'AI Prompt deleted successfully'
        });
        this.loadPrompts();
      },
      error: (error) => {
        console.error('Error deleting prompt:', error);
        this.messageService.add({
          severity: 'error',
          summary: 'Error',
          detail: 'Failed to delete AI prompt'
        });
      }
    });

    this.subscriptions.add(sub);
  }

  private markFormGroupTouched(formGroup: FormGroup): void {
    Object.keys(formGroup.controls).forEach(key => {
      const control = formGroup.get(key);
      control?.markAsTouched();
    });
  }

  truncateText(text: string | undefined, maxLength: number): string {
    if (!text) return '';
    return text.length > maxLength ? text.substring(0, maxLength) + '...' : text;
  }

  /**
   * Sets the active tab for test results display
   */
  setActiveTab(tab: 'preview' | 'raw'): void {
    this.activeTab.set(tab);
  }

  /**
   * Extracts the clean AI response text from the Gemini API response
   */
  getAiResponseText(rawResponse: string | undefined): string {
    if (!rawResponse) {
      return '';
    }
    
    try {
      const response = JSON.parse(rawResponse);
      
      // Extract text from candidates[0].content.parts[0].text
      if (response?.candidates?.[0]?.content?.parts?.[0]?.text) {
        return response.candidates[0].content.parts[0].text;
      }
      
      // Fallback: if structure is different, return the raw response
      return rawResponse;
    } catch (error) {
      // If JSON parsing fails, return the raw response
      return rawResponse;
    }
  }

  /**
   * Checks if Google Search is enabled in the tools config
   */
  isGoogleSearchEnabled(toolsConfig: string | undefined): boolean {
    if (!toolsConfig) {
      return false;
    }
    
    try {
      const toolsData = JSON.parse(toolsConfig);
      
      // New format: array with objects like [{ "googleSearch": {} }]
      if (Array.isArray(toolsData)) {
        return toolsData.some(tool => tool.googleSearch !== undefined);
      } 
      // Old format: object like { "googleSearch": true/false }
      else if (typeof toolsData === 'object' && toolsData !== null) {
        return toolsData.googleSearch || false;
      }
    } catch (error) {
      console.warn('Failed to parse tools config:', error);
    }
    
    return false;
  }

  private createForm(): FormGroup {
    const defaultContentConfig = JSON.stringify({
      "role": "user",
      "parts": [
        {
          "text": "{promptData}"
        }
      ]
    });

    const config = this.configurationData();
    
    const form = this.fb.group({
      type: ['', [Validators.required, underscoreValidator]],
      promptFunction: [''], // Not required, but when provided enables entity ID mode
      prompt: ['', [Validators.required, promptDataValidator]],
      description: [''], // Add description field
      contentConfig: [defaultContentConfig], // Hidden field with default value
      project: [config.projectId || '', Validators.required],
      location: [config.location || '', Validators.required],
      model: [config.defaultModel || '', Validators.required],
      testMode: ['testData', Validators.required], // Default to test data mode
      entityId: [''], // Validation will be conditional
      testData: [''], // Validation will be conditional
      // Generation config controls - with proper default values
      temperature: [1, [Validators.min(0), Validators.max(2)]],
      topP: [0.2, [Validators.min(0), Validators.max(1)]],
      maxOutputTokens: [8192, [Validators.min(0), Validators.max(8192)]],
      // Tools config controls
      googleSearch: [false],
      // Advanced settings
      safetySettings: ['']
    });

    // Watch for promptFunction changes to auto-switch test mode
    const promptFunctionSub = form.get('promptFunction')?.valueChanges.subscribe(value => {
      const hasFunction = !!(value && value.trim().length > 0);
      
      // Update the signal for reactivity
      this.promptFunctionValue.set(value || '');
      
      if (!hasFunction) {
        // Switch to test data mode when no function is available
        form.get('testMode')?.setValue('testData');
      }
      // Trigger change detection for computed signals
      this.cdr.detectChanges();
    });

    // Watch for model changes to update location and max tokens
    const modelSub = form.get('model')?.valueChanges.subscribe(modelValue => {
      if (modelValue) {
        // Update the signal for reactive computation
        this.selectedModelValue.set(modelValue);
        
        const selectedModel = this.geminiModels().find(m => m.value === modelValue);
        if (selectedModel) {
          // Auto-set location based on model
          form.get('location')?.setValue(selectedModel.location);
          
          // Update max tokens validation and value
          const maxTokensControl = form.get('maxOutputTokens');
          if (maxTokensControl) {
            // Update validators with new max value
            maxTokensControl.setValidators([
              Validators.min(0), 
              Validators.max(selectedModel.maxTokens)
            ]);
            maxTokensControl.updateValueAndValidity();
            
            maxTokensControl.setValue(selectedModel.maxTokens);
            
            // Force trigger change detection and update computed signals
            setTimeout(() => {
              this.cdr.detectChanges();
            }, 0);
          }
        }
      } else {
        // Reset signal when no model selected
        this.selectedModelValue.set('');
      }
    });

    // Add to subscriptions for cleanup
    if (promptFunctionSub) {
      this.subscriptions.add(promptFunctionSub);
    }
    if (modelSub) {
      this.subscriptions.add(modelSub);
    }

    return form;
  }
} 