/**
 * @fileoverview Unit tests for RequirementsValidationComponent
 * @author UNOPS Opportunity+ Development Team
 */

import { ComponentFixture, TestBed, fakeAsync, tick } from '@angular/core/testing';
import { FormGroup, FormControl, ReactiveFormsModule } from '@angular/forms';
import { of, throwError } from 'rxjs';
import { TranslateModule, TranslateService } from '@ngx-translate/core';

import { RequirementsValidationComponent } from './requirements-validation.component';
import { WorkflowService } from '../../services/workflow.service';
import { StageRequirement } from '../../models/requirement.models';

describe('RequirementsValidationComponent', () => {
  let component: RequirementsValidationComponent;
  let fixture: ComponentFixture<RequirementsValidationComponent>;
  let mockWorkflowService: jasmine.SpyObj<WorkflowService>;
  let mockTranslateService: jasmine.SpyObj<TranslateService>;

  const mockRequirements: StageRequirement[] = [
    {
      name: 'nameRequired',
      description: 'message.requirements.opportunity.nameRequired',
      fieldName: 'name',
      fieldType: 'string',
      validation: { required: true },
      isMet: false,
    },
    {
      name: 'budgetRequired',
      description: 'message.requirements.opportunity.budgetRequired',
      fieldName: 'budget',
      fieldType: 'number',
      validation: { required: true, greaterThan: 0 },
      isMet: true,
    },
    {
      name: 'sdgsRequired',
      description: 'message.requirements.opportunity.sdgsRequired',
      fieldName: 'sdgs',
      fieldType: 'array',
      validation: { required: true, minLength: 1 },
      isMet: false,
    },
  ];

  const mockStages = [
    { stage: 'IDENTIFY & PROFILE', displayName: 'Identify & Profile', sequence: 1 },
    { stage: 'GO', displayName: 'Go', sequence: 2 },
    { stage: 'NO GO', displayName: 'No Go', sequence: 3 },
    { stage: 'CANCELLED', displayName: 'Cancelled', sequence: 4 },
  ];

  beforeEach(async () => {
    mockWorkflowService = jasmine.createSpyObj('WorkflowService', [
      'getRequirementsForStageChange',
      'getWorkflowStages',
    ]);
    mockTranslateService = jasmine.createSpyObj('TranslateService', ['instant']);

    mockWorkflowService.getRequirementsForStageChange.and.returnValue(of(mockRequirements));
    mockWorkflowService.getWorkflowStages.and.returnValue(of(mockStages));
    mockTranslateService.instant.and.callFake((key: string) => key);

    await TestBed.configureTestingModule({
      imports: [RequirementsValidationComponent, ReactiveFormsModule, TranslateModule.forRoot()],
      providers: [
        { provide: WorkflowService, useValue: mockWorkflowService },
        { provide: TranslateService, useValue: mockTranslateService },
      ],
    }).compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(RequirementsValidationComponent);
    component = fixture.componentInstance;
  });

  describe('Component Initialization', () => {
    it('should create', () => {
      expect(component).toBeTruthy();
    });

    it('should load requirements on init when all inputs are provided', fakeAsync(() => {
      const form = new FormGroup({
        name: new FormControl(''),
        budget: new FormControl(1000),
        sdgs: new FormControl([]),
      });

      fixture.componentRef.setInput('entityName', 'opportunity');
      fixture.componentRef.setInput('entityId', '123');
      fixture.componentRef.setInput('currentStage', 'IDENTIFY & PROFILE');
      fixture.componentRef.setInput('formGroup', form);

      fixture.detectChanges();
      tick();

      expect(mockWorkflowService.getRequirementsForStageChange).toHaveBeenCalledWith(
        'opportunity',
        '123',
        'IDENTIFY & PROFILE'
      );
      expect(component.requirements().length).toBe(3);
    }));

    it('should load next stage display name from workflow stages', fakeAsync(() => {
      const form = new FormGroup({});

      fixture.componentRef.setInput('entityName', 'opportunity');
      fixture.componentRef.setInput('entityId', '123');
      fixture.componentRef.setInput('currentStage', 'IDENTIFY & PROFILE');
      fixture.componentRef.setInput('formGroup', form);

      fixture.detectChanges();
      tick();

      expect(mockWorkflowService.getWorkflowStages).toHaveBeenCalledWith('opportunity');
      expect(component.nextStage()).toBe('GO');
      expect(component.nextStageDisplayName()).toBe('Go');
    }));
  });

  describe('Requirement Validation', () => {
    it('should validate string required field correctly', fakeAsync(() => {
      const form = new FormGroup({
        name: new FormControl(''),
        budget: new FormControl(1000),
        sdgs: new FormControl([1, 2]),
      });

      fixture.componentRef.setInput('entityName', 'opportunity');
      fixture.componentRef.setInput('entityId', '123');
      fixture.componentRef.setInput('currentStage', 'IDENTIFY & PROFILE');
      fixture.componentRef.setInput('formGroup', form);

      fixture.detectChanges();
      tick();

      const nameReq = component.requirements().find((r) => r.name === 'nameRequired');
      expect(nameReq?.isMet).toBe(false);

      // Update form value
      form.get('name')?.setValue('Test Opportunity');
      tick();

      const updatedNameReq = component.requirements().find((r) => r.name === 'nameRequired');
      expect(updatedNameReq?.isMet).toBe(true);
    }));

    it('should validate array minLength correctly', fakeAsync(() => {
      const form = new FormGroup({
        name: new FormControl('Test'),
        budget: new FormControl(1000),
        sdgs: new FormControl([]),
      });

      fixture.componentRef.setInput('entityName', 'opportunity');
      fixture.componentRef.setInput('entityId', '123');
      fixture.componentRef.setInput('currentStage', 'IDENTIFY & PROFILE');
      fixture.componentRef.setInput('formGroup', form);

      fixture.detectChanges();
      tick();

      const sdgsReq = component.requirements().find((r) => r.name === 'sdgsRequired');
      expect(sdgsReq?.isMet).toBe(false);

      // Update form value with items
      form.get('sdgs')?.setValue([1, 2, 3] as never);
      tick();

      const updatedSdgsReq = component.requirements().find((r) => r.name === 'sdgsRequired');
      expect(updatedSdgsReq?.isMet).toBe(true);
    }));

    it('should validate number greaterThan correctly', fakeAsync(() => {
      const requirements: StageRequirement[] = [
        {
          name: 'budgetRequired',
          description: 'message.requirements.opportunity.budgetRequired',
          fieldName: 'budget',
          fieldType: 'number',
          validation: { required: true, greaterThan: 0 },
        },
      ];
      mockWorkflowService.getRequirementsForStageChange.and.returnValue(of(requirements));

      const form = new FormGroup({
        budget: new FormControl(0),
      });

      fixture.componentRef.setInput('entityName', 'opportunity');
      fixture.componentRef.setInput('entityId', '123');
      fixture.componentRef.setInput('currentStage', 'IDENTIFY & PROFILE');
      fixture.componentRef.setInput('formGroup', form);

      fixture.detectChanges();
      tick();

      const budgetReq = component.requirements().find((r) => r.name === 'budgetRequired');
      expect(budgetReq?.isMet).toBe(false);

      // Update form value with valid budget
      form.get('budget')?.setValue(50000);
      tick();

      const updatedBudgetReq = component.requirements().find((r) => r.name === 'budgetRequired');
      expect(updatedBudgetReq?.isMet).toBe(true);
    }));
  });

  describe('Computed Properties', () => {
    it('should return correct failed requirements', fakeAsync(() => {
      const form = new FormGroup({
        name: new FormControl(''),
        budget: new FormControl(1000),
        sdgs: new FormControl([]),
      });

      fixture.componentRef.setInput('entityName', 'opportunity');
      fixture.componentRef.setInput('entityId', '123');
      fixture.componentRef.setInput('currentStage', 'IDENTIFY & PROFILE');
      fixture.componentRef.setInput('formGroup', form);

      fixture.detectChanges();
      tick();

      const failed = component.failedRequirements();
      expect(failed.length).toBe(2); // name and sdgs are not met
      expect(failed.some((r) => r.name === 'nameRequired')).toBe(true);
      expect(failed.some((r) => r.name === 'sdgsRequired')).toBe(true);
    }));

    it('should return allRequirementsMet() correctly', fakeAsync(() => {
      const requirements: StageRequirement[] = [
        {
          name: 'nameRequired',
          description: 'message.requirements.opportunity.nameRequired',
          fieldName: 'name',
          fieldType: 'string',
          validation: { required: true },
        },
      ];
      mockWorkflowService.getRequirementsForStageChange.and.returnValue(of(requirements));

      const form = new FormGroup({
        name: new FormControl('Test'),
      });

      fixture.componentRef.setInput('entityName', 'opportunity');
      fixture.componentRef.setInput('entityId', '123');
      fixture.componentRef.setInput('currentStage', 'IDENTIFY & PROFILE');
      fixture.componentRef.setInput('formGroup', form);

      fixture.detectChanges();
      tick();

      expect(component.allRequirementsMet()).toBe(true);
    }));
  });

  describe('Collapsible Behavior', () => {
    it('should start collapsed by default', () => {
      const form = new FormGroup({});

      fixture.componentRef.setInput('entityName', 'opportunity');
      fixture.componentRef.setInput('entityId', '123');
      fixture.componentRef.setInput('currentStage', 'IDENTIFY & PROFILE');
      fixture.componentRef.setInput('formGroup', form);

      expect(component.isCollapsed()).toBe(true);
    });

    it('should toggle collapsed state', () => {
      const form = new FormGroup({});

      fixture.componentRef.setInput('entityName', 'opportunity');
      fixture.componentRef.setInput('entityId', '123');
      fixture.componentRef.setInput('currentStage', 'IDENTIFY & PROFILE');
      fixture.componentRef.setInput('formGroup', form);

      component.toggleCollapsed();
      expect(component.isCollapsed()).toBe(false);

      component.toggleCollapsed();
      expect(component.isCollapsed()).toBe(true);
    });
  });

  describe('Error Handling', () => {
    it('should handle API error gracefully', fakeAsync(() => {
      mockWorkflowService.getRequirementsForStageChange.and.returnValue(throwError(() => new Error('API Error')));

      const form = new FormGroup({});

      fixture.componentRef.setInput('entityName', 'opportunity');
      fixture.componentRef.setInput('entityId', '123');
      fixture.componentRef.setInput('currentStage', 'IDENTIFY & PROFILE');
      fixture.componentRef.setInput('formGroup', form);

      fixture.detectChanges();
      tick();

      expect(component.error()).toBe('Failed to load requirements');
      expect(component.requirements().length).toBe(0);
    }));
  });

  describe('Validation Message', () => {
    it('should generate correct validation message', fakeAsync(() => {
      mockTranslateService.instant.and.callFake((key: string, params?: object) => {
        if (key === 'title.opportunity') return 'Opportunity';
        if (key === 'message.requirements.title') {
          const p = params as { entity: string; nextStage: string };
          return `The ${p.entity} cannot proceed to the ${p.nextStage} stage until the following conditions are met:`;
        }
        return key;
      });

      const form = new FormGroup({});

      fixture.componentRef.setInput('entityName', 'opportunity');
      fixture.componentRef.setInput('entityId', '123');
      fixture.componentRef.setInput('currentStage', 'IDENTIFY & PROFILE');
      fixture.componentRef.setInput('formGroup', form);

      fixture.detectChanges();
      tick();

      const message = component.getValidationMessage();
      expect(message).toContain('Opportunity');
      expect(message).toContain('Go');
    }));
  });

  describe('Form Value Changes', () => {
    it('should revalidate when form values change', fakeAsync(() => {
      const requirements: StageRequirement[] = [
        {
          name: 'nameRequired',
          description: 'message.requirements.opportunity.nameRequired',
          fieldName: 'name',
          fieldType: 'string',
          validation: { required: true },
        },
      ];
      mockWorkflowService.getRequirementsForStageChange.and.returnValue(of(requirements));

      const form = new FormGroup({
        name: new FormControl(''),
      });

      fixture.componentRef.setInput('entityName', 'opportunity');
      fixture.componentRef.setInput('entityId', '123');
      fixture.componentRef.setInput('currentStage', 'IDENTIFY & PROFILE');
      fixture.componentRef.setInput('formGroup', form);

      fixture.detectChanges();
      tick();

      expect(component.requirements()[0].isMet).toBe(false);

      form.get('name')?.setValue('Test Value');
      tick();

      expect(component.requirements()[0].isMet).toBe(true);
    }));

    it('should emit validationChanged when requirements status changes', fakeAsync(() => {
      const requirements: StageRequirement[] = [
        {
          name: 'nameRequired',
          description: 'message.requirements.opportunity.nameRequired',
          fieldName: 'name',
          fieldType: 'string',
          validation: { required: true },
        },
      ];
      mockWorkflowService.getRequirementsForStageChange.and.returnValue(of(requirements));

      const form = new FormGroup({
        name: new FormControl(''),
      });

      fixture.componentRef.setInput('entityName', 'opportunity');
      fixture.componentRef.setInput('entityId', '123');
      fixture.componentRef.setInput('currentStage', 'IDENTIFY & PROFILE');
      fixture.componentRef.setInput('formGroup', form);

      const validationChangedSpy = spyOn(component.validationChanged, 'emit');

      fixture.detectChanges();
      tick();

      form.get('name')?.setValue('Test Value');
      tick();

      expect(validationChangedSpy).toHaveBeenCalled();
    }));
  });
});
