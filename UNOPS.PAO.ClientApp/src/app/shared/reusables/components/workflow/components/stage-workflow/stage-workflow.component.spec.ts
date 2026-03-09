/**
 * @fileoverview Unit tests for StageWorkflowComponent
 * @author UNOPS Opportunity+ Development Team
 */

import { ComponentFixture, TestBed, fakeAsync, tick } from '@angular/core/testing';
import { of } from 'rxjs';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { DialogService } from 'primeng/dynamicdialog';
import { ConfirmationService } from 'primeng/api';
import { provideNoopAnimations } from '@angular/platform-browser/animations';

import { StageWorkflowComponent } from './stage-workflow.component';
import { WorkflowService } from '../../services/workflow.service';
import { IFeedbackDialogService } from '../workflow/workflow.component';

describe('StageWorkflowComponent', () => {
  let component: StageWorkflowComponent;
  let fixture: ComponentFixture<StageWorkflowComponent>;
  let mockWorkflowService: jasmine.SpyObj<WorkflowService>;
  let mockTranslateService: jasmine.SpyObj<TranslateService>;
  let mockFeedbackService: jasmine.SpyObj<IFeedbackDialogService>;

  const mockStages = [
    { stage: 'IDENTIFY & PROFILE', displayName: 'Identify & Profile', sequence: 1 },
    { stage: 'GO', displayName: 'Go', sequence: 2 },
    { stage: 'NO GO', displayName: 'No Go', sequence: 3 },
    { stage: 'CANCELLED', displayName: 'Cancelled', sequence: 4 },
  ];

  const mockWorkflowState = {
    stage: 'IDENTIFY & PROFILE',
    displayName: 'Identify & Profile',
    comment: '',
    isInWorkflow: false,
    nextActions: [],
  };

  beforeEach(async () => {
    mockWorkflowService = jasmine.createSpyObj('WorkflowService', [
      'getWorkFlowForEntity',
      'getNextWorkFlowActionsForARecordById',
      'getWorkflowDetails',
      'getStageChangeHistory',
      'cancelOpportunity',
      'reopenOpportunity',
    ]);
    mockTranslateService = jasmine.createSpyObj('TranslateService', ['instant']);
    mockFeedbackService = jasmine.createSpyObj('IFeedbackDialogService', [
      'showConfirmDialog',
      'showSuccessToast',
      'showInfoToast',
    ]);

    mockWorkflowService.getWorkFlowForEntity.and.returnValue(of(mockStages));
    mockWorkflowService.getNextWorkFlowActionsForARecordById.and.returnValue(of(mockWorkflowState));
    mockWorkflowService.getStageChangeHistory.and.returnValue(of([]));
    mockWorkflowService.cancelOpportunity.and.returnValue(of({} as any));
    mockWorkflowService.reopenOpportunity.and.returnValue(of({} as any));
    mockWorkflowService.getWorkflowDetails.and.returnValue(of({ approvers: [], canRecall: false, canApprove: false, pendingStage: null }));
    mockTranslateService.instant.and.callFake((key: string) => key);

    await TestBed.configureTestingModule({
      imports: [StageWorkflowComponent, TranslateModule.forRoot()],
      providers: [
        { provide: WorkflowService, useValue: mockWorkflowService },
        { provide: TranslateService, useValue: mockTranslateService },
        { provide: DialogService, useValue: { open: () => ({ onClose: of(null) }) } },
        { provide: ConfirmationService, useValue: { confirm: (opts?: { accept?: () => void }) => opts?.accept?.() } },
        provideNoopAnimations(),
      ],
    }).compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(StageWorkflowComponent);
    component = fixture.componentInstance;
    component.feedbackDialogService = mockFeedbackService;
  });

  describe('Component Creation', () => {
    it('should create', () => {
      expect(component).toBeTruthy();
    });
  });

  describe('getDisplayStages - Happy Path Filtering', () => {
    beforeEach(() => {
      // Initialize stages
      component.stages.set(
        mockStages.map((s) => ({
          label: s.displayName,
          name: s.stage,
          value: s.sequence,
        }))
      );
    });

    it('should show only IDENTIFY & PROFILE and GO for default stage', () => {
      component.setCurrentStageName('IDENTIFY & PROFILE');

      const displayStages = component.displayStages();

      expect(displayStages.length).toBe(2);
      expect(displayStages[0]['name']).toBe('IDENTIFY & PROFILE');
      expect(displayStages[1]['name']).toBe('GO');
    });

    it('should show only IDENTIFY & PROFILE and GO when in GO stage', () => {
      component.setCurrentStageName('GO');

      const displayStages = component.displayStages();

      expect(displayStages.length).toBe(2);
      expect(displayStages[0]['name']).toBe('IDENTIFY & PROFILE');
      expect(displayStages[1]['name']).toBe('GO');
    });

    it('should show only IDENTIFY & PROFILE and NO GO when in NO GO stage', () => {
      component.setCurrentStageName('NO GO');

      const displayStages = component.displayStages();

      expect(displayStages.length).toBe(2);
      expect(displayStages[0]['name']).toBe('IDENTIFY & PROFILE');
      expect(displayStages[1]['name']).toBe('NO GO');
    });

    it('should show only IDENTIFY & PROFILE and CANCELLED when in CANCELLED stage', () => {
      component.setCurrentStageName('CANCELLED');

      const displayStages = component.displayStages();

      expect(displayStages.length).toBe(2);
      expect(displayStages[0]['name']).toBe('IDENTIFY & PROFILE');
      expect(displayStages[1]['name']).toBe('CANCELLED');
    });
  });

  describe('displayStageIndex', () => {
    beforeEach(() => {
      component.stages.set(
        mockStages.map((s) => ({
          label: s.displayName,
          name: s.stage,
          value: s.sequence,
        }))
      );
    });

    it('should return 0 for IDENTIFY & PROFILE stage', () => {
      component.setCurrentStageName('IDENTIFY & PROFILE');

      expect(component.displayStageIndex()).toBe(0);
    });

    it('should return 1 for GO stage', () => {
      component.setCurrentStageName('GO');

      expect(component.displayStageIndex()).toBe(1);
    });

    it('should return 1 for NO GO stage (second in filtered list)', () => {
      component.setCurrentStageName('NO GO');

      expect(component.displayStageIndex()).toBe(1);
    });

    it('should return 1 for CANCELLED stage (second in filtered list)', () => {
      component.setCurrentStageName('CANCELLED');

      expect(component.displayStageIndex()).toBe(1);
    });
  });

  describe('Cancel Button Visibility', () => {
    beforeEach(() => {
      component.stages.set(
        mockStages.map((s) => ({
          label: s.displayName,
          name: s.stage,
          value: s.sequence,
        }))
      );
    });

    it('should show Cancel button when OM and in IDENTIFY & PROFILE and not in workflow', () => {
      fixture.componentRef.setInput('isOpportunityManager', true);
      component.setCurrentStageName('IDENTIFY & PROFILE');
      component.workflowData.set({ isInWorkflow: false });

      expect(component.canCancel()).toBe(true);
    });

    it('should hide Cancel button when not OM', () => {
      fixture.componentRef.setInput('isOpportunityManager', false);
      fixture.componentRef.setInput('canChangeStage', false);
      component.setCurrentStageName('IDENTIFY & PROFILE');
      component.workflowData.set({ isInWorkflow: false });

      expect(component.canCancel()).toBe(false);
    });

    it('should hide Cancel button when in workflow', () => {
      fixture.componentRef.setInput('isOpportunityManager', true);
      component.setCurrentStageName('IDENTIFY & PROFILE');
      component.workflowData.set({ isInWorkflow: true });

      expect(component.canCancel()).toBe(false);
    });

    it('should hide Cancel button when in GO stage', () => {
      fixture.componentRef.setInput('isOpportunityManager', true);
      component.setCurrentStageName('GO');
      component.workflowData.set({ isInWorkflow: false });

      expect(component.canCancel()).toBe(false);
    });
  });

  describe('Reopen Button Visibility', () => {
    beforeEach(() => {
      component.stages.set(
        mockStages.map((s) => ({
          label: s.displayName,
          name: s.stage,
          value: s.sequence,
        }))
      );
    });

    it('should show Reopen button when OM and in NO GO stage', () => {
      fixture.componentRef.setInput('isOpportunityManager', true);
      component.setCurrentStageName('NO GO');

      expect(component.canReopen()).toBe(true);
    });

    it('should show Reopen button when OM and in CANCELLED stage', () => {
      fixture.componentRef.setInput('isOpportunityManager', true);
      component.setCurrentStageName('CANCELLED');

      expect(component.canReopen()).toBe(true);
    });

    it('should hide Reopen button when not OM', () => {
      fixture.componentRef.setInput('isOpportunityManager', false);
      fixture.componentRef.setInput('canChangeStage', false);
      component.setCurrentStageName('NO GO');

      expect(component.canReopen()).toBe(false);
    });

    it('should hide Reopen button when in IDENTIFY & PROFILE stage', () => {
      fixture.componentRef.setInput('isOpportunityManager', true);
      component.setCurrentStageName('IDENTIFY & PROFILE');

      expect(component.canReopen()).toBe(false);
    });

    it('should hide Reopen button when in GO stage', () => {
      fixture.componentRef.setInput('isOpportunityManager', true);
      component.setCurrentStageName('GO');

      expect(component.canReopen()).toBe(false);
    });
  });

  describe('Reopen Reason Required', () => {
    it('should require reason for reopen from CANCELLED stage', () => {
      component.setCurrentStageName('CANCELLED');

      expect(component.reopenRequiresReason()).toBe(true);
    });

    it('should not require reason for reopen from NO GO stage', () => {
      component.setCurrentStageName('NO GO');

      expect(component.reopenRequiresReason()).toBe(false);
    });
  });

  describe('Cancel Dialog', () => {
    beforeEach(() => {
      fixture.componentRef.setInput('entityName', 'opportunity');
      fixture.componentRef.setInput('entityId', '123');
    });

    it('should open cancel dialog', () => {
      component.openCancelDialog();

      expect(component.showCancelDialog()).toBe(true);
      expect(component.cancelReason()).toBe('');
    });

    it('should close cancel dialog', () => {
      component.showCancelDialog.set(true);
      component.cancelReason.set('some reason');

      component.closeCancelDialog();

      expect(component.showCancelDialog()).toBe(false);
      expect(component.cancelReason()).toBe('');
    });

    it('should call cancelOpportunity service on confirm', fakeAsync(() => {
      mockWorkflowService.cancelOpportunity.and.returnValue(of({} as any));
      component.cancelReason.set('No longer needed');

      component.confirmCancel();
      tick();

      expect(mockWorkflowService.cancelOpportunity).toHaveBeenCalledWith('123', 'No longer needed');
      expect(mockFeedbackService.showSuccessToast).toHaveBeenCalled();
    }));

    it('should show info toast if reason is empty', () => {
      component.cancelReason.set('');

      component.confirmCancel();

      expect(mockFeedbackService.showInfoToast).toHaveBeenCalled();
      expect(mockWorkflowService.cancelOpportunity).not.toHaveBeenCalled();
    });
  });

  describe('Reopen Dialog', () => {
    beforeEach(() => {
      fixture.componentRef.setInput('entityName', 'opportunity');
      fixture.componentRef.setInput('entityId', '123');
    });

    it('should open reopen dialog', () => {
      component.openReopenDialog();

      expect(component.showReopenDialog()).toBe(true);
      expect(component.reopenReason()).toBe('');
    });

    it('should close reopen dialog', () => {
      component.showReopenDialog.set(true);
      component.reopenReason.set('some reason');

      component.closeReopenDialog();

      expect(component.showReopenDialog()).toBe(false);
      expect(component.reopenReason()).toBe('');
    });

    it('should call reopenOpportunity service on confirm from NO GO (optional reason)', fakeAsync(() => {
      mockWorkflowService.reopenOpportunity.and.returnValue(of({} as any));
      component.setCurrentStageName('NO GO');
      component.reopenReason.set('');

      component.confirmReopen();
      tick();

      expect(mockWorkflowService.reopenOpportunity).toHaveBeenCalledWith('123', undefined);
      expect(mockFeedbackService.showSuccessToast).toHaveBeenCalled();
    }));

    it('should call reopenOpportunity service with reason from CANCELLED', fakeAsync(() => {
      mockWorkflowService.reopenOpportunity.and.returnValue(of({} as any));
      component.setCurrentStageName('CANCELLED');
      component.reopenReason.set('Circumstances changed');

      component.confirmReopen();
      tick();

      expect(mockWorkflowService.reopenOpportunity).toHaveBeenCalledWith('123', 'Circumstances changed');
      expect(mockFeedbackService.showSuccessToast).toHaveBeenCalled();
    }));

    it('should require reason for reopen from CANCELLED', () => {
      component.setCurrentStageName('CANCELLED');
      component.reopenReason.set('');

      component.confirmReopen();

      expect(mockFeedbackService.showInfoToast).toHaveBeenCalled();
      expect(mockWorkflowService.reopenOpportunity).not.toHaveBeenCalled();
    });
  });
});
