/**
 * @fileoverview Unit tests for WorkflowService
 * @author UNOPS Opportunity+ Development Team
 */

import { TestBed } from '@angular/core/testing';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';

import { WorkflowService } from './workflow.service';
import { StageRequirement } from '../models/requirement.models';
import { WorkflowStageModel, WorkflowStateModel, WorkflowActionModel, WorkflowHistoryModel } from '../models/workflow.models';

describe('WorkflowService', () => {
  let service: WorkflowService;
  let httpMock: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [HttpClientTestingModule],
      providers: [WorkflowService],
    });

    service = TestBed.inject(WorkflowService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  describe('Service Creation', () => {
    it('should be created', () => {
      expect(service).toBeTruthy();
    });
  });

  describe('getWorkflowStages', () => {
    it('should call correct endpoint', () => {
      const mockStages: WorkflowStageModel[] = [
        { stage: 'IDENTIFY & PROFILE', displayName: 'Identify & Profile', sequence: 1 },
        { stage: 'GO', displayName: 'Go', sequence: 2 },
        { stage: 'NO GO', displayName: 'No Go', sequence: 3 },
        { stage: 'CANCELLED', displayName: 'Cancelled', sequence: 4 },
      ];

      service.getWorkflowStages('opportunity').subscribe((stages) => {
        expect(stages).toEqual(mockStages);
        expect(stages.length).toBe(4);
      });

      const req = httpMock.expectOne('/api/workflow/opportunity');
      expect(req.request.method).toBe('GET');
      req.flush(mockStages);
    });

    it('should use configured apiBaseUrl', () => {
      service.configure({ apiBaseUrl: '/custom-api' });

      service.getWorkflowStages('opportunity').subscribe();

      const req = httpMock.expectOne('/custom-api/workflow/opportunity');
      expect(req.request.method).toBe('GET');
      req.flush([]);
    });
  });

  describe('getRequirementsForStageChange', () => {
    const mockRequirements: StageRequirement[] = [
      {
        name: 'nameRequired',
        description: 'message.requirements.opportunity.nameRequired',
        fieldName: 'name',
        fieldType: 'string',
        validation: { required: true },
      },
      {
        name: 'budgetRequired',
        description: 'message.requirements.opportunity.budgetRequired',
        fieldName: 'budget',
        fieldType: 'number',
        validation: { required: true, greaterThan: 0 },
      },
    ];

    it('should call correct endpoint without currentStage', () => {
      service.getRequirementsForStageChange('opportunity', '123').subscribe((requirements) => {
        expect(requirements).toEqual(mockRequirements);
        expect(requirements.length).toBe(2);
      });

      const req = httpMock.expectOne('/api/workflow/opportunity/123/requirements');
      expect(req.request.method).toBe('GET');
      req.flush(mockRequirements);
    });

    it('should call correct endpoint with currentStage', () => {
      service.getRequirementsForStageChange('opportunity', '123', 'IDENTIFY & PROFILE').subscribe((requirements) => {
        expect(requirements).toEqual(mockRequirements);
      });

      const req = httpMock.expectOne('/api/workflow/opportunity/123/requirements?currentStage=IDENTIFY%20%26%20PROFILE');
      expect(req.request.method).toBe('GET');
      req.flush(mockRequirements);
    });

    it('should return StageRequirement[] response', () => {
      service.getRequirementsForStageChange('opportunity', '456').subscribe((requirements) => {
        expect(Array.isArray(requirements)).toBe(true);
        expect(requirements[0].name).toBe('nameRequired');
        expect(requirements[0].fieldType).toBe('string');
        expect(requirements[0].validation?.required).toBe(true);
      });

      const req = httpMock.expectOne('/api/workflow/opportunity/456/requirements');
      req.flush(mockRequirements);
    });

    it('should handle empty requirements array', () => {
      service.getRequirementsForStageChange('opportunity', '789').subscribe((requirements) => {
        expect(requirements).toEqual([]);
        expect(requirements.length).toBe(0);
      });

      const req = httpMock.expectOne('/api/workflow/opportunity/789/requirements');
      req.flush([]);
    });
  });

  describe('getNextWorkFlowActionsForARecordById', () => {
    it('should call correct endpoint', () => {
      const mockState: WorkflowStateModel = {
        stage: 'IDENTIFY & PROFILE',
        displayName: 'Identify & Profile',
        comment: '',
        isInWorkflow: false,
        nextActions: [
          {
            actionName: 'Submit',
            newStage: 'GO',
            sequence: 1,
            comment: 'optional',
            requiresApproval: true,
          },
        ],
      };

      service.getNextWorkFlowActionsForARecordById('opportunity', '123').subscribe((state) => {
        expect(state).toEqual(mockState);
        expect(state.nextActions?.length).toBe(1);
      });

      const req = httpMock.expectOne('/api/workflow/opportunity/123');
      expect(req.request.method).toBe('GET');
      req.flush(mockState);
    });
  });

  describe('getWorkflowDetails', () => {
    it('should call correct endpoint', () => {
      const mockDetails = {
        nextStage: 'GO',
        canRecall: false,
        recallComment: '',
        canApprove: true,
        approvalComment: '',
        canReject: true,
        rejectionComment: '',
        approvers: [],
      };

      service.getWorkflowDetails('opportunity', '123').subscribe((details) => {
        expect(details).toEqual(mockDetails);
      });

      const req = httpMock.expectOne('/api/workflow/opportunity/123/details');
      expect(req.request.method).toBe('GET');
      req.flush(mockDetails);
    });
  });

  describe('getStageChangeHistory', () => {
    it('should call correct endpoint', () => {
      const mockHistory: WorkflowHistoryModel[] = [
        {
          fromStage: 'IDENTIFY & PROFILE',
          toStage: 'GO',
          performedOn: new Date(),
          action: 'Approved',
          comment: 'Looks good',
          requiresApproval: true,
        },
      ];

      service.getStageChangeHistory('opportunity', '123').subscribe((history) => {
        expect(history).toEqual(mockHistory);
        expect(history.length).toBe(1);
      });

      const req = httpMock.expectOne('/api/workflow/opportunity/123/history');
      expect(req.request.method).toBe('GET');
      req.flush(mockHistory);
    });
  });

  describe('changeWorkflow', () => {
    it('should call correct endpoint with POST', () => {
      const request: WorkflowActionModel = {
        entityName: 'opportunity',
        entityId: 123,
        newStage: 'GO',
        comment: 'Approved for Go decision',
      };

      const mockResponse: WorkflowStateModel = {
        stage: 'GO',
        displayName: 'Go',
        comment: '',
        isInWorkflow: false,
      };

      service.changeWorkflow(request).subscribe((response) => {
        expect(response).toEqual(mockResponse);
      });

      const req = httpMock.expectOne('/api/workflow/submit');
      expect(req.request.method).toBe('POST');
      expect(req.request.body).toEqual(request);
      req.flush(mockResponse);
    });
  });

  describe('getWorkFlowForEntity (deprecated)', () => {
    it('should call getWorkflowStages internally', () => {
      const mockStages: WorkflowStageModel[] = [
        { stage: 'IDENTIFY & PROFILE', displayName: 'Identify & Profile', sequence: 1 },
      ];

      service.getWorkFlowForEntity('opportunity').subscribe((stages) => {
        expect(stages).toEqual(mockStages);
      });

      const req = httpMock.expectOne('/api/workflow/opportunity');
      expect(req.request.method).toBe('GET');
      req.flush(mockStages);
    });
  });

  describe('Configuration', () => {
    it('should use default apiBaseUrl', () => {
      service.getWorkflowStages('opportunity').subscribe();

      const req = httpMock.expectOne('/api/workflow/opportunity');
      expect(req.request.url).toBe('/api/workflow/opportunity');
      req.flush([]);
    });

    it('should use custom apiBaseUrl after configure', () => {
      service.configure({ apiBaseUrl: '/v2/api' });

      service.getWorkflowStages('opportunity').subscribe();

      const req = httpMock.expectOne('/v2/api/workflow/opportunity');
      expect(req.request.url).toBe('/v2/api/workflow/opportunity');
      req.flush([]);
    });
  });

  describe('cancelOpportunity', () => {
    it('should call correct endpoint with POST', () => {
      const mockResponse: WorkflowStateModel = {
        stage: 'CANCELLED',
        displayName: 'Cancelled',
        comment: '',
        isInWorkflow: false,
      };

      service.cancelOpportunity('123', 'No longer needed').subscribe((response) => {
        expect(response).toEqual(mockResponse);
      });

      const req = httpMock.expectOne('/api/workflow/cancel');
      expect(req.request.method).toBe('POST');
      expect(req.request.body).toEqual({
        entityName: 'opportunity',
        entityId: 123,
        comment: 'No longer needed',
      });
      req.flush(mockResponse);
    });
  });

  describe('reopenOpportunity', () => {
    it('should call correct endpoint with POST and comment', () => {
      const mockResponse: WorkflowStateModel = {
        stage: 'IDENTIFY & PROFILE',
        displayName: 'Identify & Profile',
        comment: '',
        isInWorkflow: false,
      };

      service.reopenOpportunity('456', 'Circumstances changed').subscribe((response) => {
        expect(response).toEqual(mockResponse);
      });

      const req = httpMock.expectOne('/api/workflow/reopen');
      expect(req.request.method).toBe('POST');
      expect(req.request.body).toEqual({
        entityName: 'opportunity',
        entityId: 456,
        comment: 'Circumstances changed',
      });
      req.flush(mockResponse);
    });

    it('should call correct endpoint without comment', () => {
      const mockResponse: WorkflowStateModel = {
        stage: 'IDENTIFY & PROFILE',
        displayName: 'Identify & Profile',
        comment: '',
        isInWorkflow: false,
      };

      service.reopenOpportunity('789').subscribe((response) => {
        expect(response).toEqual(mockResponse);
      });

      const req = httpMock.expectOne('/api/workflow/reopen');
      expect(req.request.method).toBe('POST');
      expect(req.request.body).toEqual({
        entityName: 'opportunity',
        entityId: 789,
        comment: undefined,
      });
      req.flush(mockResponse);
    });
  });

  describe('submitForGoDecision', () => {
    it('should call submit endpoint with confirmation flags', () => {
      const request = {
        entityName: 'opportunity',
        entityId: 123,
        newStage: 'GO',
        confirmedNonOMSubmission: true,
        acknowledgedStatement: true,
        additionalRemarks: 'Ready for review',
      };

      const mockResponse = {
        success: true,
        newStage: 'GO',
      };

      service.submitForGoDecision(request).subscribe((response) => {
        expect(response.success).toBe(true);
      });

      const req = httpMock.expectOne('/api/workflow/submit');
      expect(req.request.method).toBe('POST');
      expect(req.request.body).toEqual(request);
      req.flush(mockResponse);
    });

    it('should handle RequiresConfirmation response', () => {
      const request = {
        entityName: 'opportunity',
        entityId: 123,
        newStage: 'GO',
      };

      const mockResponse = {
        success: false,
        requiresConfirmation: true,
        confirmationType: 'NonOMSubmitter',
        confirmationMessage: 'You currently hold a [Stakeholder] role...',
      };

      service.submitForGoDecision(request).subscribe((response) => {
        expect(response.success).toBe(false);
        expect(response.requiresConfirmation).toBe(true);
        expect(response.confirmationType).toBe('NonOMSubmitter');
      });

      const req = httpMock.expectOne('/api/workflow/submit');
      req.flush(mockResponse);
    });

    it('should handle RequiresAcknowledgment response', () => {
      const request = {
        entityName: 'opportunity',
        entityId: 123,
        newStage: 'GO',
        confirmedNonOMSubmission: true,
      };

      const mockResponse = {
        success: false,
        requiresAcknowledgment: true,
        acknowledgmentText: 'All known information...',
      };

      service.submitForGoDecision(request).subscribe((response) => {
        expect(response.success).toBe(false);
        expect(response.requiresAcknowledgment).toBe(true);
        expect(response.acknowledgmentText).toBeDefined();
      });

      const req = httpMock.expectOne('/api/workflow/submit');
      req.flush(mockResponse);
    });
  });
});
