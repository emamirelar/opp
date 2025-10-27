import { TestBed } from '@angular/core/testing';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { WorkflowService } from './workflow.service';

describe('WorkflowService', () => {
  let service: WorkflowService;
  let httpMock: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [HttpClientTestingModule],
      providers: [WorkflowService]
    });

    service = TestBed.inject(WorkflowService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  it('should get workflow for entity', (done) => {
    const entityName = 'Contact';
    const mockWorkflow = [
      { stage: 'draft', displayName: 'Draft' },
      { stage: 'published', displayName: 'Published' }
    ];

    expect(service.isLoading()).toBe(false);

    service.getWorkFlowForEntity(entityName).subscribe(response => {
      expect(response).toEqual(mockWorkflow);
      expect(service.isLoading()).toBe(false);
      done();
    });

    expect(service.isLoading()).toBe(true);

    const req = httpMock.expectOne('/api/workflow/' + entityName);
    expect(req.request.method).toBe('GET');
    req.flush(mockWorkflow);
  });

  it('should get next workflow actions for a record', (done) => {
    const entityName = 'Partner';
    const recordId = 123;
    const mockActions = [
      { action: 'approve', displayName: 'Approve' },
      { action: 'reject', displayName: 'Reject' }
    ];

    service.getNextWorkFlowAtionsForARecordById(entityName, recordId).subscribe(response => {
      expect(response).toEqual(mockActions);
      expect(service.isLoading()).toBe(false);
      done();
    });

    const req = httpMock.expectOne('/api/workflow/' + entityName + '/' + recordId);
    expect(req.request.method).toBe('GET');
    req.flush(mockActions);
  });

  it('should change workflow', (done) => {
    const requestJson = {
      entityName: 'Contact',
      recordId: 456,
      newStage: 'published',
      comment: 'Approved'
    };
    const mockResponse = { success: true };

    service.changeWorkflow(requestJson).subscribe(response => {
      expect(response).toEqual(mockResponse);
      expect(service.isLoading()).toBe(false);
      done();
    });

    const req = httpMock.expectOne('/api/workflow');
    expect(req.request.method).toBe('POST');
    expect(req.request.body).toEqual(requestJson);
    req.flush(mockResponse);
  });

  it('should set isLoading to false on error', (done) => {
    const entityName = 'Contact';

    service.getWorkFlowForEntity(entityName).subscribe({
      next: () => fail('should have errored'),
      error: () => {
        expect(service.isLoading()).toBe(false);
        done();
      }
    });

    const req = httpMock.expectOne('/api/workflow/' + entityName);
    req.error(new ProgressEvent('error'));
  });

  // TODO: Add tests for workflow validation
  // TODO: Add tests for workflow history tracking
});

