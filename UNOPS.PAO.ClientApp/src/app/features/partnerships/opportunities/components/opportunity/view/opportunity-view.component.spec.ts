/**
 * @fileoverview Unit tests for OpportunityViewComponent - Workflow Integration
 * @author UNOPS Opportunity+ System Development Team
 */

import { ComponentFixture, TestBed, fakeAsync, tick } from '@angular/core/testing';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { Router } from '@angular/router';
import { ActivatedRoute } from '@angular/router';
import { Location } from '@angular/common';
import { RouterTestingModule } from '@angular/router/testing';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { of, throwError } from 'rxjs';
import { signal } from '@angular/core';

import { OpportunityViewComponent } from './opportunity-view.component';
import { OpportunityService } from '../../../services/opportunity.service';
import { FeedbackDialogService } from '@shared/services/ui';
import { PermissionUtilityService, AuthService } from '@core/services/auth';
import { PageContextService } from '@shared/services/utils';
import { ValuesService } from '@app/shared/services/api/values.service';
import { WorkflowService } from '@shared/reusables/components/workflow/services/workflow.service';
import { GoogleOAuthService } from '@core/services/auth/google-oauth.service';
import { ConfirmationService } from 'primeng/api';
import { MarkdownService } from 'ngx-markdown';
import { provideNoopAnimations } from '@angular/platform-browser/animations';
import { Opportunity } from '@shared/models/opportunity.model';
import { StageWorkflowComponent } from '@shared/reusables/components/workflow/components/stage-workflow/stage-workflow.component';
import { DrivePickerService } from '@shared/services/integration/drive-picker.service';

describe('OpportunityViewComponent - Workflow Integration', () => {
  let component: OpportunityViewComponent;
  let fixture: ComponentFixture<OpportunityViewComponent>;
  let httpMock: HttpTestingController;
  let opportunityService: jasmine.SpyObj<OpportunityService>;
  let router: jasmine.SpyObj<Router>;
  let activatedRoute: Partial<ActivatedRoute>;
  let feedbackDialogService: jasmine.SpyObj<FeedbackDialogService>;
  let permissionUtilityService: jasmine.SpyObj<PermissionUtilityService>;

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
      'generateOpportunityImages',
      'getExecutivesForOpportunity',
      'getRiskLookups',
      'getRiskCategories',
      'getHighRiskChecklist',
      'getDSTRisks',
      'getDSTRecommendations',
      'getSimilarOpportunities',
      'getSimilarProjects',
      'getRelevantPeople',
      'getSourceInteractions',
      'getFrameworkStatus',
      'extractProductsAndServices',
      'getCollaboratorExpertises',
    ]);
    opportunityServiceSpy.generateOpportunityImages.and.returnValue(of(mockOpportunity));
    opportunityServiceSpy.getExecutivesForOpportunity.and.returnValue(of([]));
    opportunityServiceSpy.getRiskLookups.and.returnValue(of({ riskTypes: [], riskCategories: [] }));
    opportunityServiceSpy.getRiskCategories.and.returnValue(of([]));
    opportunityServiceSpy.getHighRiskChecklist.and.returnValue(of([]));
    opportunityServiceSpy.getDSTRisks.and.returnValue(of({ risks: [], highRiskAnalysis: null }));
    opportunityServiceSpy.getDSTRecommendations.and.returnValue(of({ recommendations: [], suggestions: [] }));
    opportunityServiceSpy.getSimilarOpportunities.and.returnValue(
      of({ opportunities: [], similarOpportunities: [] }),
    );
    opportunityServiceSpy.getSimilarProjects.and.returnValue(
      of({ projects: [], similarProjects: [] }),
    );
    opportunityServiceSpy.getRelevantPeople.and.returnValue(
      of({ people: [], relevantPeople: [] }),
    );
    opportunityServiceSpy.getSourceInteractions.and.returnValue(of([]));
    opportunityServiceSpy.getFrameworkStatus.and.returnValue(of({}));
    opportunityServiceSpy.extractProductsAndServices.and.returnValue(of([]));
    opportunityServiceSpy.getCollaboratorExpertises.and.returnValue(of([]));
    const routerSpy = jasmine.createSpyObj('Router', ['navigate', 'createUrlTree']);
    Object.defineProperty(routerSpy, 'events', { value: of({}), configurable: true });
    routerSpy.createUrlTree.and.returnValue({} as any);
    const feedbackDialogServiceSpy = jasmine.createSpyObj('FeedbackDialogService', [
      'showSuccessToast',
      'showErrorToast',
    ]);
    const permissionUtilityServiceSpy = jasmine.createSpyObj('PermissionUtilityService', [
      'createInstancePermissions',
      'canUpdate',
    ]);
    const mockRecordPermissions = signal({
      canUpdate: true,
      canDelete: false,
    });
    permissionUtilityServiceSpy.createInstancePermissions.and.returnValue({
      recordPermissions: mockRecordPermissions,
      loadPermissions: jasmine.createSpy('loadPermissions'),
    } as any);
    permissionUtilityServiceSpy.canUpdate.and.returnValue(true);

    const paramMap = {
      get: (key: string) => (key === 'recordId' ? '123' : key === 'section' ? null : null),
      has: (key: string) => key === 'recordId',
      getAll: () => [],
      keys: ['recordId'],
    };
    activatedRoute = {
      params: of({ recordId: '123' }),
      paramMap: of(paramMap),
      queryParams: of({}),
      snapshot: { paramMap } as any,
    };

    await TestBed.configureTestingModule({
      imports: [
        OpportunityViewComponent,
        HttpClientTestingModule,
        TranslateModule.forRoot(),
        RouterTestingModule,
      ],
      providers: [
        { provide: OpportunityService, useValue: opportunityServiceSpy },
        { provide: ActivatedRoute, useValue: activatedRoute },
        { provide: Location, useValue: {} },
        { provide: FeedbackDialogService, useValue: feedbackDialogServiceSpy },
        { provide: PermissionUtilityService, useValue: permissionUtilityServiceSpy },
        {
          provide: PageContextService,
          useValue: {
            setComponentData: () => {},
            clearComponentData: () => {},
          },
        },
        {
          provide: ValuesService,
          useValue: {
            getConfig: () => of({}),
            getOrganizationUnits: () => of([]),
            getProposedInitiativeTypes: () => of([]),
            getOutputs: () => of([]),
            getDistinctLevel0: () => [],
            getDistinctLevel1: () => [],
            getDistinctLevel2: () => [],
            getDistinctLevel3: () => [],
            getDistinctLevel4: () => [],
            getFilteredOutputsByLevels: () => [],
            semanticSearchOutputs: () => of([]),
            getSDGs: () => of([]),
            getUNOPSMissions: () => of([]),
            getSDGTargets: () => of([]),
            getSDGIndicators: () => of([]),
            getUNCFIndicators: () => of([]),
            getCountries: () => of([]),
            dynamicSearchCountries: () => of([]),
            getPartners: () => of([]),
            getCurrencies: () => of([]),
            getContacts: () => of([]),
            getEntityUserRolesByOrgUnits: () => of([]),
            getEntityRoles: () => of([]),
            getInternalUsers: () => of([]),
            getOrgUnitIdsForCountries: () => of([]),
            getChildOrgUnitIdsForHubRegion: () => of([]),
            getOpportunityOrganizationUnits: () => of([]),
            getSuggestedOrgUnits: () => of({ suggestedOrgUnitIds: [], primarySuggestionId: null, suggestionReason: null }),
          },
        },
        { provide: ConfirmationService, useValue: { confirm: () => {} } },
        {
          provide: MarkdownService,
          useValue: {
            parse: () => '',
            compile: () => '',
            render: () => {},
            reload$: of(undefined),
            getSource: () => of(''),
          },
        },
        {
          provide: AuthService,
          useValue: { user: () => of([{ type: 'email', value: 'test@test.com' }]) },
        },
        { provide: GoogleOAuthService, useValue: {} },
        {
          provide: WorkflowService,
          useValue: {
            getWorkFlowForEntity: () => of([]),
            getWorkflowStages: () => of([]),
            getRequirementsForStageChange: () => of([]),
            getNextWorkFlowActionsForARecordById: () => of({}),
            getWorkflowDetails: () => of({}),
            getStageChangeHistory: () => of([]),
            cancelOpportunity: () => of({}),
            reopenOpportunity: () => of({}),
          },
        },
        {
          provide: DrivePickerService,
          useValue: { pickFiles: () => of([]), openPicker: () => of([]), isPickerReady: () => false },
        },
        provideNoopAnimations(),
      ],
    }).compileComponents();

    httpMock = TestBed.inject(HttpTestingController);
    fixture = TestBed.createComponent(OpportunityViewComponent);
    component = fixture.componentInstance;
    const translateService = TestBed.inject(TranslateService);
    spyOn(translateService, 'instant').and.callFake((key: string) =>
      ({ 'message.success': 'Success', 'message.workflow.submitSuccess': 'Stage change successful', 'message.error': 'Error', 'message.opportunity.loadFailed': 'Failed to load opportunity' }[key] || key));
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
  });

  afterEach(() => {
    if (httpMock) {
      // Flush any pending requests from child components to avoid verify() failures
      try {
        const pending = httpMock.match(() => true);
        pending.forEach((req) => {
          try {
            const url = req.request?.url ?? '';
            if (url.includes('configuration')) req.flush({});
            else req.flush([]);
          } catch {
            req.flush([]);
          }
        });
      } catch {
        // Ignore flush errors
      }
      httpMock.verify();
    }
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

    it('should render StageWorkflowComponent when opportunity is loaded', fakeAsync(() => {
      fixture.detectChanges();
      tick(100);
      fixture.detectChanges();

      const workflowComponent = fixture.nativeElement.querySelector('app-stage-workflow');
      expect(workflowComponent).toBeTruthy();
    }));

    it('should pass correct inputs to StageWorkflowComponent', fakeAsync(() => {
      component.opportunity.set(mockOpportunity);
      component.recordId = '123';
      component.loading.set(false);
      fixture.detectChanges();
      tick();

      const workflowComponent = fixture.nativeElement.querySelector('app-stage-workflow');
      expect(workflowComponent).toBeTruthy();
      expect(workflowComponent.getAttribute('ng-reflect-entity-name')).toBe('opportunity');
      expect(workflowComponent.getAttribute('ng-reflect-entity-id')).toBe('123');
    }));

    it('should bind canChangeStage computed property to workflow component', () => {
      component.opportunity.set(mockOpportunity);
      // Avoid detectChanges to prevent _loadRecordDetails from overwriting opportunity
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
      const canChangeStage = component.canChangeStage();
      expect(canChangeStage).toBe(false);
    });

    it('should set canChangeStage to false when opportunity has no id', () => {
      const oppWithoutId = {
        ...mockOpportunity,
        id: 0, // Use 0 instead of undefined since id is required as number
      };
      component.opportunity.set(oppWithoutId);
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

    it('should complete when handleStageChangeSuccess is called', () => {
      expect(() => component.handleStageChangeSuccess()).not.toThrow();
    });

    it('should reload opportunity data after stage change', (done) => {
      const updatedOpportunity = {
        ...mockOpportunity,
        stage: 'GO',
        workflowStatus: 'None',
        isInWorkflow: false,
      };

      opportunityService.getOpportunityById.and.returnValue(of(updatedOpportunity));
      (component as any).reloadOpportunity?.and?.callThrough?.();

      component.opportunity.set(mockOpportunity);
      component.recordId = '123';
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
    it('should handle workflow API responses correctly', (done) => {
      opportunityService.getOpportunityById.and.returnValue(of(mockOpportunity));
      opportunityService.getInsights.and.returnValue(of({ insights: [], suggestions: [] }));

      fixture.detectChanges();

      setTimeout(() => {
        expect(opportunityService.getOpportunityById).toHaveBeenCalledWith(123);
        expect(component.opportunity()).toEqual(mockOpportunity);
        done();
      }, 100);
    });

    it('should handle workflow API errors gracefully', (done) => {
      opportunityService.getOpportunityById.and.returnValue(
        throwError(() => new Error('API Error')),
      );
      opportunityService.getInsights.and.returnValue(of({ insights: [], suggestions: [] }));

      fixture.detectChanges();

      setTimeout(() => {
        expect(feedbackDialogService.showErrorToast).toHaveBeenCalled();
        done();
      }, 100);
    });
  });

  describe('Workflow Component ViewChild', () => {
    it('should have stageWorkflowComponent ViewChild reference available after view init', () => {
      // Create a mock StageWorkflowComponent
      const mockWorkflowComponent = {
        entityName: 'opportunity',
        entityId: '123',
        canChangeStage: true,
      } as unknown as Partial<StageWorkflowComponent>;

      // Simulate ViewChild being set
      component.stageWorkflowComponent = mockWorkflowComponent as StageWorkflowComponent;

      expect(component.stageWorkflowComponent).toBeDefined();
      expect(component.stageWorkflowComponent?.entityName).toBe('opportunity');
    });
  });

  describe('Workflow Stage Display', () => {
    it('should have stage available when opportunity has stage', () => {
      component.opportunity.set(mockOpportunity);
      component.loading.set(false);
      fixture.detectChanges();

      expect(component.opportunity()?.stage).toBe('IDENTIFY & PROFILE');
      expect(component.canChangeStage()).toBe(true);
    });

    it('should not have stage when opportunity has no stage', () => {
      const oppWithoutStage = {
        ...mockOpportunity,
        stage: null,
      };
      component.opportunity.set(oppWithoutStage);
      component.loading.set(false);
      fixture.detectChanges();

      expect(component.opportunity()?.stage).toBeNull();
    });
  });
});
