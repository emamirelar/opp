import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ActivatedRoute, Router } from '@angular/router';
import { HttpClientTestingModule } from '@angular/common/http/testing';
import { TranslateModule, TranslateStore } from '@ngx-translate/core';
import { MessageService } from 'primeng/api';
import { of } from 'rxjs';

import { FeatureBaseComponent } from './feature-base.component';

describe('FeatureBaseComponent', () => {
  let component: FeatureBaseComponent;
  let fixture: ComponentFixture<FeatureBaseComponent>;

  const mockActivatedRoute = {
    params: of({}),
    queryParams: of({})
  };

  const mockRouter = {
    navigate: jasmine.createSpy('navigate')
  };

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [
        FeatureBaseComponent,
        HttpClientTestingModule,
        TranslateModule.forRoot()
      ],
      providers: [
        { provide: ActivatedRoute, useValue: mockActivatedRoute },
        { provide: Router, useValue: mockRouter },
        TranslateStore,
        MessageService
      ]
    })
    .compileComponents();

    fixture = TestBed.createComponent(FeatureBaseComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
