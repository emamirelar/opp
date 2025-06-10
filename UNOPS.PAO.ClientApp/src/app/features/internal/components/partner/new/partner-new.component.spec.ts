import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ActivatedRoute, Router } from '@angular/router';
import { HttpClientTestingModule } from '@angular/common/http/testing';
import { TranslateModule, TranslateStore } from '@ngx-translate/core';
import { MessageService } from 'primeng/api';
import { DialogService } from 'primeng/dynamicdialog';
import { provideNoopAnimations } from '@angular/platform-browser/animations';
import { of } from 'rxjs';

import { PartnerNewComponent } from './partner-new.component';

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
        DialogService,
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
