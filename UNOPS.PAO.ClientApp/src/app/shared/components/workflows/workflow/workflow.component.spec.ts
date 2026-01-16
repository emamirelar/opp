import { ComponentFixture, TestBed } from '@angular/core/testing';
import { WorkflowComponent } from './workflow.component';
import { WorkflowService } from '@shared/services/domain/workflow.service';
import { FeedbackDialogService } from '@shared/services/ui/feedback-dialog.service';
import { of, throwError } from 'rxjs';

describe('WorkflowComponent', () => {
  let component: WorkflowComponent;
  let fixture: ComponentFixture<WorkflowComponent>;
  let mockWorkflowService: jasmine.SpyObj<WorkflowService>;
  let mockFeedbackService: jasmine.SpyObj<FeedbackDialogService>;

  const mockWorkflowActions = {
    nextActions: [
      { newStage: 'Approved', actionName: 'Approve', commentRequired: false },
      { newStage: 'Rejected', actionName: 'Reject', commentRequired: true },
      { newStage: 'OnHold', actionName: 'Put on Hold', commentRequired: false }
    ]
  };

  beforeEach(async () => {
    mockWorkflowService = jasmine.createSpyObj('WorkflowService', [
      'getNextWorkFlowAtionsForARecordById',
      'changeWorkflow'
    ]);
    mockFeedbackService = jasmine.createSpyObj('FeedbackDialogService', [
      'showSuccessToast',
      'showErrorToast'
    ]);

    mockWorkflowService.getNextWorkFlowAtionsForARecordById.and.returnValue(of(mockWorkflowActions));
    mockWorkflowService.changeWorkflow.and.returnValue(of(mockWorkflowActions));

    await TestBed.configureTestingModule({
      imports: [WorkflowComponent],
      providers: [
        { provide: WorkflowService, useValue: mockWorkflowService },
        { provide: FeedbackDialogService, useValue: mockFeedbackService }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(WorkflowComponent);
    component = fixture.componentInstance;
    fixture.componentRef.setInput('entityName', 'Partner');
    fixture.componentRef.setInput('entityId', '123');
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  describe('initialization', () => {
    it('should have default values', () => {
      expect(component.primaryeStageLabel).toBe('');
      expect(component.primaryeStageName).toBe('');
      expect(component.primaryStageChangeCommentRequired).toBeFalse();
      expect(component.showCommentDialog).toBeFalse();
      expect(component.items).toEqual([]);
      expect(component.nextStage()).toBe('');
    });

    it('should load workflows on init', () => {
      expect(mockWorkflowService.getNextWorkFlowAtionsForARecordById).toHaveBeenCalledWith('Partner', '123');
    });

    it('should initialize UI with workflow actions', () => {
      expect(component.primaryeStageName).toBe('Approved');
      expect(component.primaryeStageLabel).toBe('Approve');
      expect(component.primaryStageChangeCommentRequired).toBeFalse();
      expect(component.items.length).toBe(2); // Secondary actions
    });
  });

  describe('_initialiseUI', () => {
    it('should set primary action from first item', () => {
      component._initialiseUI(mockWorkflowActions.nextActions);

      expect(component.primaryeStageName).toBe('Approved');
      expect(component.primaryeStageLabel).toBe('Approve');
      expect(component.primaryStageChangeCommentRequired).toBeFalse();
    });

    it('should create menu items for secondary actions', () => {
      component._initialiseUI(mockWorkflowActions.nextActions);

      expect(component.items.length).toBe(2);
      expect(component.items[0].label).toBe('Reject');
      expect(component.items[1].label).toBe('Put on Hold');
    });

    it('should attach command handlers to menu items', () => {
      spyOn(component, '_executeStageChange');
      component._initialiseUI(mockWorkflowActions.nextActions);

      component.items[0].command!({});

      expect(component._executeStageChange).toHaveBeenCalledWith('Rejected', true);
    });

    it('should handle single action', () => {
      const singleAction = [mockWorkflowActions.nextActions[0]];
      
      component._initialiseUI(singleAction);

      expect(component.primaryeStageName).toBe('Approved');
      expect(component.items.length).toBe(0);
    });

    it('should handle empty actions array', () => {
      component._initialiseUI([]);

      expect(component.primaryeStageName).toBe('');
      expect(component.items.length).toBe(0);
    });
  });

  describe('_executeStageChange', () => {
    beforeEach(() => {
    });

    it('should set next stage', () => {
      component._executeStageChange('Approved', false);

      expect(component.nextStage()).toBe('Approved');
    });

    it('should show comment dialog if comment is required', () => {
      component._executeStageChange('Rejected', true);

      expect(component.showCommentDialog).toBeTrue();
    });

    it('should perform stage change immediately if comment not required', () => {
      spyOn(component, '_performStageChange');
      
      component._executeStageChange('Approved', false);

      expect(component._performStageChange).toHaveBeenCalled();
    });

    it('should call beforeStageChange if provided', (done) => {
      component.beforeStageChange = jasmine.createSpy().and.returnValue(Promise.resolve(true));
      
      component._executeStageChange('Approved', false);

      setTimeout(() => {
        expect(component.beforeStageChange).toHaveBeenCalled();
        done();
      }, 10);
    });

    it('should not proceed if beforeStageChange returns false', (done) => {
      component.beforeStageChange = jasmine.createSpy().and.returnValue(Promise.resolve(false));
      spyOn(component, '_performStageChange');
      
      component._executeStageChange('Approved', false);

      setTimeout(() => {
        expect(component._performStageChange).not.toHaveBeenCalled();
        done();
      }, 10);
    });

    it('should show comment dialog after beforeStageChange if comment required', (done) => {
      component.beforeStageChange = jasmine.createSpy().and.returnValue(Promise.resolve(true));
      
      component._executeStageChange('Rejected', true);

      setTimeout(() => {
        expect(component.showCommentDialog).toBeTrue();
        done();
      }, 10);
    });
  });

  describe('_performStageChange', () => {
    beforeEach(() => {
      component.nextStage.set('Approved');
    });

    it('should call workflow service with correct parameters', () => {
      component._performStageChange();

      expect(mockWorkflowService.changeWorkflow).toHaveBeenCalledWith({
        entityName: 'Partner',
        id: '123',
        newStage: 'Approved',
        comment: ''
      });
    });

    it('should include comment if provided', () => {
      component._performStageChange('This is a comment');

      expect(mockWorkflowService.changeWorkflow).toHaveBeenCalledWith(
        jasmine.objectContaining({ comment: 'This is a comment' })
      );
    });

    it('should show success toast on completion', () => {
      component._performStageChange();

      expect(mockFeedbackService.showSuccessToast).toHaveBeenCalledWith({
        detail: 'Stage changed successfully!'
      });
    });

    it('should reinitialize UI with new actions', () => {
      spyOn(component, '_initialiseUI');
      
      component._performStageChange();

      expect(component._initialiseUI).toHaveBeenCalledWith(mockWorkflowActions.nextActions);
    });

    it('should emit stageChangeSuccess event', (done) => {
      spyOn(component.stageChangeSuccess, 'emit');

      component._performStageChange();

      expect(component.stageChangeSuccess.emit).toHaveBeenCalled();
      done();
    });

    it('should handle errors gracefully', () => {
      mockWorkflowService.changeWorkflow.and.returnValue(throwError(() => new Error('Failed')));

      expect(() => component._performStageChange()).not.toThrow();
    });
  });

  describe('_handleOnPrimaryStageClick', () => {
    beforeEach(() => {
      component.primaryeStageName = 'Approved';
      component.primaryStageChangeCommentRequired = false;
    });

    it('should execute stage change for primary action', () => {
      spyOn(component, '_executeStageChange');
      
      component._handleOnPrimaryStageClick();

      expect(component._executeStageChange).toHaveBeenCalledWith('Approved', false);
    });

    it('should pass correct comment requirement', () => {
      component.primaryStageChangeCommentRequired = true;
      spyOn(component, '_executeStageChange');
      
      component._handleOnPrimaryStageClick();

      expect(component._executeStageChange).toHaveBeenCalledWith('Approved', true);
    });
  });

  describe('handleOnCommentSave', () => {
    beforeEach(() => {
      component.showCommentDialog = true;
    });

    it('should perform stage change with comment', () => {
      spyOn(component, '_performStageChange');
      
      component.handleOnCommentSave('User comment');

      expect(component._performStageChange).toHaveBeenCalledWith('User comment');
    });

    it('should close comment dialog', () => {
      component.handleOnCommentSave('User comment');

      expect(component.showCommentDialog).toBeFalse();
    });

    it('should handle empty comment', () => {
      spyOn(component, '_performStageChange');
      
      component.handleOnCommentSave('');

      expect(component._performStageChange).toHaveBeenCalledWith('');
    });
  });

  describe('input properties', () => {
    it('should accept entityName input', () => {
      fixture.componentRef.setInput('entityName', 'Contact');
      fixture.detectChanges();

      expect(component.entityName()).toBe('Contact');
    });

    it('should accept entityId input', () => {
      fixture.componentRef.setInput('entityId', '456');
      fixture.detectChanges();

      expect(component.entityId()).toBe('456');
    });

    it('should accept disabled input', () => {
      component.disabled = true;
      fixture.detectChanges();

      expect(component.disabled).toBeTrue();
    });

    it('should accept beforeStageChange callback', () => {
      const callback = () => Promise.resolve(true);
      component.beforeStageChange = callback;

      expect(component.beforeStageChange).toBe(callback);
    });
  });

  describe('output events', () => {
    it('should emit stageChangeSuccess on successful change', (done) => {
      component.nextStage.set('Approved');

      spyOn(component.stageChangeSuccess, 'emit');

      component._performStageChange();

      expect(component.stageChangeSuccess.emit).toHaveBeenCalled();
      done();
    });
  });

  describe('edge cases', () => {
    it('should handle workflow with no secondary actions', () => {
      const singleAction = { nextActions: [mockWorkflowActions.nextActions[0]] };
      mockWorkflowService.getNextWorkFlowAtionsForARecordById.and.returnValue(of(singleAction));
      component.ngOnInit();

      expect(component.items.length).toBe(0);
      expect(component.primaryeStageLabel).toBe('Approve');
    });

    it('should handle long stage names', () => {
      const longAction = {
        nextActions: [{
          newStage: 'VeryLongStageNameThatExceedsNormalLength',
          actionName: 'Very Long Action Name That Should Be Displayed',
          commentRequired: false
        }]
      };
      mockWorkflowService.getNextWorkFlowAtionsForARecordById.and.returnValue(of(longAction));
      component.ngOnInit();

      expect(component.primaryeStageName).toBe('VeryLongStageNameThatExceedsNormalLength');
    });

    it('should handle special characters in comments', () => {
      component.nextStage.set('Approved');

      component._performStageChange('Comment with "quotes" and <html> & symbols');

      expect(mockWorkflowService.changeWorkflow).toHaveBeenCalledWith(
        jasmine.objectContaining({
          comment: 'Comment with "quotes" and <html> & symbols'
        })
      );
    });

    it('should handle multiple rapid stage changes', () => {
      component._executeStageChange('Stage1', false);
      component._executeStageChange('Stage2', false);
      component._executeStageChange('Stage3', false);

      expect(component.nextStage()).toBe('Stage3');
    });

    it('should handle missing workflow data', () => {
      mockWorkflowService.getNextWorkFlowAtionsForARecordById.and.returnValue(of({ nextActions: [] }));

      expect(() => component.ngOnInit()).not.toThrow();
    });
  });

  describe('signal reactivity', () => {
    it('should update nextStage signal reactively', () => {
      expect(component.nextStage()).toBe('');

      component.nextStage.set('NewStage');

      expect(component.nextStage()).toBe('NewStage');
    });

    it('should maintain signal state across operations', () => {
      component.nextStage.set('Stage1');
      expect(component.nextStage()).toBe('Stage1');

      component._executeStageChange('Stage2', false);
      expect(component.nextStage()).toBe('Stage2');
    });
  });
});

