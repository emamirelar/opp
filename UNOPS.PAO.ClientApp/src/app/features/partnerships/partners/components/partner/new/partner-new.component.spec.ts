import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ActivatedRoute, Router } from '@angular/router';
import { HttpClientTestingModule } from '@angular/common/http/testing';
import { TranslateModule, TranslateStore } from '@ngx-translate/core';
import { MessageService, ConfirmationService } from 'primeng/api';
import { DialogService } from 'primeng/dynamicdialog';
import { provideNoopAnimations } from '@angular/platform-browser/animations';
import { of } from 'rxjs';
import { signal } from '@angular/core';

import { PartnerNewComponent } from './partner-new.component';
import { CachedDataService } from '@shared/services/utils';
import { FeedbackDialogService } from '@shared/services/ui';
import { PartnerService } from '@partnerships/partners/services/partner.service';
import { LanguageService } from '@shared/services/utils';

const mockCachedDataService = {
  allPartnerStatus: signal([]),
  allPartnerNewEngagement: signal([]),
  allYesNo: signal([]),
  allPartnerLevyApplies: signal([]),
  allPartnerReasonForLevyNot: signal([]),
  allPartnerLevyTreatment: signal([]),
  allPartnerScope: signal([])
};

describe('NewPartnerComponent', () => {
  let component: PartnerNewComponent;
  let fixture: ComponentFixture<PartnerNewComponent>;

  const mockActivatedRoute = {
    params: of({}),
    queryParams: of({}),
    data: of({})
  };

  const mockRouter = {
    navigate: jasmine.createSpy('navigate')
  };

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [
        PartnerNewComponent,
        HttpClientTestingModule,
        TranslateModule.forRoot()
      ],
      providers: [
        { provide: ActivatedRoute, useValue: mockActivatedRoute },
        { provide: Router, useValue: mockRouter },
        TranslateStore,
        MessageService,
        { provide: DialogService, useValue: { open: () => ({ onClose: of(null) }) } },
        { provide: ConfirmationService, useValue: { confirm: (opts?: { accept?: () => void }) => opts?.accept?.() } },
        { provide: CachedDataService, useValue: mockCachedDataService },
        { provide: FeedbackDialogService, useValue: jasmine.createSpyObj('FeedbackDialogService', ['showConfirmDialog', 'showSuccessToast', 'showErrorToast']) },
        { provide: PartnerService, useValue: Object.assign(jasmine.createSpyObj('PartnerService', ['create', 'update']), { isLoading: signal(false) }) },
        { provide: LanguageService, useValue: jasmine.createSpyObj('LanguageService', ['getCurrentLanguage']) },
        provideNoopAnimations()
      ]
    })
    .compileComponents();

    fixture = TestBed.createComponent(PartnerNewComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
