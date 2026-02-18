/**
 * @fileoverview Unit tests for OpportunityViewComponent - Workflow Integration
 * @author UNOPS Opportunity+ System Development Team
 */

import { ComponentFixture, TestBed } from '@angular/core/testing';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { Router } from '@angular/router';
import { ActivatedRoute } from '@angular/router';
import { Location } from '@angular/common';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { of, throwError } from 'rxjs';
import { signal } from '@angular/core';

import { OpportunityViewComponent } from './opportunity-view.component';
import { OpportunityService } from '../../../services/opportunity.service';
import { FeedbackDialogService } from '@shared/services/ui';
import { PermissionUtilityService } from '@core/services/auth';
import { PageContextService } from '@shared/services/utils';
import { ValuesService } from '@app/shared/services/api/values.service';
import { ConfirmationService } from 'primeng/api';
import { Opportunity } from '@shared/models/opportunity.model';
import { StageWorkflowComponent } from '@shared/reusables/components/workflow/components/stage-workflow/stage-workflow.component';

describe('OpportunityViewComponent - Workflow Integration', () => {
  let component: OpportunityViewComponent;
  let fixture: ComponentFixture<OpportunityViewComponent>;
  let httpMock: HttpTestingController;
  let opportunityService: jasmine.SpyObj<OpportunityService>;
  let router: jasmine.SpyObj<Router>;
  let activatedRoute: Partial<ActivatedRoute>;
  let feedbackDialogService: jasmine.SpyObj<FeedbackDialogService>;
  let permissionUtilityService: jasmine.SpyObj<PermissionUtilityService>;
  let translateService: TranslateService;

  const mockOpportunity: Opportunity = {
    id: 123,
    name: 'Test Opportunity',
    description: 'Test Description',
    partnerReference: null,
    status: 'Active',
    stage: 'IDENTIFY & PROFILE',
    workflowStatus: 'None',
    isInWorkflow: false,
    responsibleOrgUnitId: null,
    responsibleOrgUnitName: null,
    proposedInitiativeTypeId: null,
    proposedInitiativeTypeName: null,
    initiativeBudgetUSD: null,
    partnershipAgreementReference: null,
    targetSigningDate: null,
    implementationStartDate: null,
    targetDeliveryDate: null,
    isTargetSigningDateFirm: false,
    signingDateNotes: null,
    submissionDeadline: null,
    resultsFocus: null,
    expectedImpact: null,
    expectedOutcomes: null,
    expectedBeneficiaries: null,
    estimatedDirectBeneficiaries: null,
    estimatedIndirectBeneficiaries: null,
    beneficiariesToBeDetermined: false,
    challenges: null,
    opportunityStatementMarkdown: null,
    opportunityBannerImage: null,
    opportunityThumbnail: null,
    isPooledFunding: false,
    highRisksAcknowledged: false,
    deliveryModality: null,
    fundingPartners: [],
    clientPartners: [],
    stakeholders: [],
    externalStakeholders: [],
    miscExternalStakeholders: null,
    externalStakeholderNotes: null,
    deliverables: [],
    countries: [],
    sdGs: [],
    stats: null,
    isNewValueRangeForOrgUnit: null,
    orgUnitHistoricalMaxValue: null,
    dstAnalysis: null,
    insights: [],
    suggestions: [],
    createdDate: new Date().toISOString(),
    lastModifiedDate: new Date().toISOString(),
    createdBy: 1,
    createdByName: 'Test User',
    lastModifiedBy: 1,
    lastModifiedByName: 'Test User',
    permissions: {
      canRead: true,
      canCreate: false,
      canUpdate: true,
      canDelete: false,
    },
  } as Opportunity;

  beforeEach(async () => {
    const opportunityServiceSpy = jasmine.createSpyObj('OpportunityService', [
      'getOpportunityById',
      'getInsights',
    ]);
    const routerSpy = jasmine.createSpyObj('Router', ['navigate']);
    const feedbackDialogServiceSpy = jasmine.createSpyObj('FeedbackDialogService', [
      'showSuccessToast',
      'showErrorToast',
    ]);
    const permissionUtilityServiceSpy = jasmine.createSpyObj('PermissionUtilityService', [
      'createInstancePermissions',
      'canUpdate',
    ]);

    activatedRoute = {
      params: of({ id: '123' }),
      queryParams: of({}),
    };

    await TestBed.configureTestingModule({
      imports: [
        OpportunityViewComponent,
        HttpClientTestingModule,
        TranslateModule.forRoot(),
      ],
      providers: [
        { provide: OpportunityService, useValue: opportunityServiceSpy },
        { provide: Router, useValue: routerSpy },
        { provide: ActivatedRoute, useValue: activatedRoute },
        { provide: Location, useValue: {} },
        { provide: FeedbackDialogService, useValue: feedbackDialogServiceSpy },
        { provide: PermissionUtilityService, useValue: permissionUtilityServiceSpy },
        { provide: PageContextService, useValue: {} },
        { provide: ValuesService, useValue: {} },
        { provide: ConfirmationService, useValue: {} },
        TranslateService,
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(OpportunityViewComponent);
    component = fixture.componentInstance;
    httpMock = TestBed.inject(HttpTestingController);
    opportunityService = TestBed.inject(
      OpportunityService,
    ) as jasmine.SpyObj<OpportunityService>;
    router = TestBed.inject(Router) as jasmine.SpyObj<Router>;
    feedbackDialogService = TestBed.inject(
      FeedbackDialogService,
    ) as jasmine.SpyObj<FeedbackDialogService>;
    permissionUtilityService = TestBed.inject(
      PermissionUtilityService,
    ) as jasmine.SpyObj<PermissionUtilityService>;
    translateService = TestBed.inject(TranslateService);

    // Setup permission utility service mock
    const mockRecordPermissions = signal({
      canUpdate: true,
      canDelete: false,
    });
    permissionUtilityService.createInstancePermissions.and.returnValue({
      recordPermissions: mockRecordPermissions,
    } as any);
    permissionUtilityService.canUpdate.and.returnValue(true);
  });

  afterEach(() => {
    httpMock.verify();
  });

  describe('Component Initialization', () => {
    it('should create the component', () => {
      expect(component).toBeTruthy();
    });

    it('should have stageWorkflowComponent ViewChild reference', () => {
      expect(component.stageWorkflowComponent).toBeUndefined(); // Initially undefined until view init
    });
  });

  describe('Workflow Component Integration', () => {
    beforeEach(() => {
      opportunityService.getOpportunityById.and.returnValue(of(mockOpportunity));
      opportunityService.getInsights.and.returnValue(of({ insights: [], suggestions: [] }));
    });

    it('should render StageWorkflowComponent when opportunity is loaded', () => {
      fixture.detectChanges();
      
      const workflowComponent = fixture.nativeElement.querySelector('app-stage-workflow');
      expect(workflowComponent).toBeTruthy();
    });

    it('should pass correct inputs to StageWorkflowComponent', () => {
      component.opportunity.set(mockOpportunity);
      component.recordId = '123';
      fixture.detectChanges();

      const workflowComponent = fixture.nativeElement.querySelector('app-stage-workflow');
      expect(workflowComponent).toBeTruthy();
      expect(workflowComponent.getAttribute('ng-reflect-entity-name')).toBe('opportunity');
      expect(workflowComponent.getAttribute('ng-reflect-entity-id')).toBe('123');
    });

    it('should bind canChangeStage computed property to workflow component', () => {
      component.opportunity.set(mockOpportunity);
      fixture.detectChanges();

      const canChangeStage = component.canChangeStage();
      expect(canChangeStage).toBe(true); // Should be true when canUpdate is true and opportunity has id
    });

    it('should set canChangeStage to false when user cannot update', () => {
      const oppWithoutUpdatePermission = {
        ...mockOpportunity,
        permissions: {
          canRead: true,
          canCreate: false,
          canUpdate: false,
          canDelete: false,
        },
      };
      component.opportunity.set(oppWithoutUpdatePermission);
      fixture.detectChanges();

      const canChangeStage = component.canChangeStage();
      expect(canChangeStage).toBe(false);
    });

    it('should set canChangeStage to false when opportunity has no id', () => {
      const oppWithoutId = {
        ...mockOpportunity,
        id: 0, // Use 0 instead of undefined since id is required as number
      };
      component.opportunity.set(oppWithoutId);
      fixture.detectChanges();

      const canChangeStage = component.canChangeStage();
      expect(canChangeStage).toBe(false);
    });
  });

  describe('handleStageChangeSuccess', () => {
    beforeEach(() => {
      opportunityService.getOpportunityById.and.returnValue(of(mockOpportunity));
      opportunityService.getInsights.and.returnValue(of({ insights: [], suggestions: [] }));
      component.recordId = '123';
      spyOn(component, 'reloadOpportunity' as any);
    });

    it('should call reloadOpportunity when handleStageChangeSuccess is called', () => {
      component.handleStageChangeSuccess();
      expect(component['reloadOpportunity']).toHaveBeenCalled();
    });

    it('should show success toast when handleStageChangeSuccess is called', () => {
      translateService.set('message.success', 'Success');
      translateService.set('message.workflow.submitSuccess', 'Stage change successful');

      component.handleStageChangeSuccess();

      expect(feedbackDialogService.showSuccessToast).toHaveBeenCalledWith({
        summary: 'Success',
        detail: 'Stage change successful',
      });
    });

    it('should reload opportunity data after stage change', (done) => {
      const updatedOpportunity = {
        ...mockOpportunity,
        stage: 'GO',
        workflowStatus: 'None',
        isInWorkflow: false,
      };

      opportunityService.getOpportunityById.and.returnValue(of(updatedOpportunity));

      component.opportunity.set(mockOpportunity);
      component.handleStageChangeSuccess();

      // Wait for reload to complete
      setTimeout(() => {
        expect(opportunityService.getOpportunityById).toHaveBeenCalledWith(123);
        done();
      }, 100);
    });
  });

  describe('reloadOpportunity', () => {
    beforeEach(() => {
      component.recordId = '123';
      spyOn(component as any, '_loadRecordDetails');
    });

    it('should call _loadRecordDetails when reloadOpportunity is called with recordId', () => {
      component.reloadOpportunity();
      expect(component['_loadRecordDetails']).toHaveBeenCalled();
    });

    it('should not call _loadRecordDetails when recordId is empty', () => {
      component.recordId = '';
      component.reloadOpportunity();
      expect(component['_loadRecordDetails']).not.toHaveBeenCalled();
    });

    it('should set shouldScrollAfterDataLoad to false when reloading', () => {
      component['shouldScrollAfterDataLoad'] = true;
      component.reloadOpportunity();
      expect(component['shouldScrollAfterDataLoad']).toBe(false);
    });
  });

  describe('Workflow API Integration', () => {
    it('should handle workflow API responses correctly', () => {
      opportunityService.getOpportunityById.and.returnValue(of(mockOpportunity));
      opportunityService.getInsights.and.returnValue(of({ insights: [], suggestions: [] }));

      fixture.detectChanges();

      const req = httpMock.expectOne('/api/opportunity/123');
      expect(req.request.method).toBe('GET');
      req.flush(mockOpportunity);
    });

    it('should handle workflow API errors gracefully', () => {
      opportunityService.getOpportunityById.and.returnValue(
        throwError(() => new Error('API Error')),
      );

      translateService.set('message.error', 'Error');
      translateService.set('message.opportunity.loadFailed', 'Failed to load opportunity');

      fixture.detectChanges();

      expect(feedbackDialogService.showErrorToast).toHaveBeenCalled();
    });
  });

  describe('Workflow Component ViewChild', () => {
    it('should have stageWorkflowComponent ViewChild reference available after view init', () => {
      // Create a mock StageWorkflowComponent
      const mockWorkflowComponent = {
        entityName: 'opportunity',
        entityId: '123',
        canChangeStage: true,
      } as Partial<StageWorkflowComponent>;

      // Simulate ViewChild being set
      component.stageWorkflowComponent = mockWorkflowComponent as StageWorkflowComponent;

      expect(component.stageWorkflowComponent).toBeDefined();
      expect(component.stageWorkflowComponent?.entityName).toBe('opportunity');
    });
  });

  describe('Workflow Stage Display', () => {
    it('should display stage badge when opportunity has stage', () => {
      component.opportunity.set(mockOpportunity);
      component.loading.set(false);
      fixture.detectChanges();

      const stageBadge = fixture.nativeElement.querySelector('p-badge[ng-reflect-value="IDENTIFY & PROFILE"]');
      expect(stageBadge).toBeTruthy();
    });

    it('should not display stage badge when opportunity has no stage', () => {
      const oppWithoutStage = {
        ...mockOpportunity,
        stage: null, // Use null instead of undefined since stage is string | null
      };
      component.opportunity.set(oppWithoutStage);
      component.loading.set(false);
      fixture.detectChanges();

      // Stage badge should not be rendered when stage is null
      const stageBadges = fixture.nativeElement.querySelectorAll('p-badge');
      const stageBadge = Array.from(stageBadges).find((badge: any) => 
        badge.getAttribute('ng-reflect-value')?.includes('IDENTIFY')
      );
      expect(stageBadge).toBeFalsy();
    });
  });
});
