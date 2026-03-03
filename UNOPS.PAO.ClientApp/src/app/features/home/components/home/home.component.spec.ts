import { ComponentFixture, TestBed } from '@angular/core/testing';
import { HttpClientTestingModule } from '@angular/common/http/testing';
import { TranslateModule, TranslateLoader, TranslateFakeLoader } from '@ngx-translate/core';
import { DialogService } from 'primeng/dynamicdialog';
import { HomeComponent } from './home.component';
import { of, Subject } from 'rxjs';
import { DocumentService } from '@shared/services/api/document.service';
import { OpportunityService } from '@partnerships/opportunities/services/opportunity.service';
import { PartnerService } from '@partnerships/partners/services/partner.service';
import { ContactService } from '@partnerships/contacts/services/contact.service';
import { InteractionService } from '@partnerships/interactions/services/interaction.service';
import { GlobalFilterService } from '@core/services/filters';
import { PermissionService } from '@core/services/auth';
import { FeedbackDialogService } from '@shared/services/ui/feedback-dialog.service';
import { LayoutService } from '@layouts/services/layout.service';
import { WorkflowService } from '@shared/reusables/components/workflow/services/workflow.service';
import { DrivePickerService } from '@shared/services/integration/drive-picker.service';

describe('HomeComponent', () => {
  let component: HomeComponent;
  let fixture: ComponentFixture<HomeComponent>;

  beforeEach(async () => {
    const filtersChanged$ = new Subject<void>();
    const mockGlobalFilterService = { filtersChanged$ };
    const mockPermissionService = jasmine.createSpyObj('PermissionService', ['getEntityPermissions']);
    mockPermissionService.getEntityPermissions.and.returnValue(of({ permissions: { canCreate: false } }));
    const mockWorkflowService = jasmine.createSpyObj('WorkflowService', ['getPendingApprovalsForUser']);
    mockWorkflowService.getPendingApprovalsForUser.and.returnValue(of([]));

    await TestBed.configureTestingModule({
      imports: [
        HomeComponent,
        HttpClientTestingModule,
        TranslateModule.forRoot({ loader: { provide: TranslateLoader, useClass: TranslateFakeLoader } })
      ],
      providers: [
        { provide: DialogService, useValue: {} },
        { provide: DocumentService, useValue: {} },
        { provide: OpportunityService, useValue: {} },
        { provide: PartnerService, useValue: {} },
        { provide: ContactService, useValue: {} },
        { provide: InteractionService, useValue: {} },
        { provide: GlobalFilterService, useValue: mockGlobalFilterService },
        { provide: PermissionService, useValue: mockPermissionService },
        { provide: FeedbackDialogService, useValue: {} },
        { provide: LayoutService, useValue: {} },
        { provide: WorkflowService, useValue: mockWorkflowService },
        { provide: DrivePickerService, useValue: { pickFiles: () => of([]), openPicker: () => of([]) } }
      ]
    })
    .compileComponents();

    fixture = TestBed.createComponent(HomeComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});

