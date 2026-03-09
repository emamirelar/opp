import { ComponentFixture, TestBed } from '@angular/core/testing';
import { EntityGridComponent } from './entity-grid.component';
import { HttpClientTestingModule } from '@angular/common/http/testing';
import { TranslateModule, TranslateLoader, TranslateFakeLoader } from '@ngx-translate/core';
import { EntityConfigurationService } from '@shared/services/api/entity-configuration.service';
import { Router } from '@angular/router';
import { of } from 'rxjs';

describe('EntityGridComponent', () => {
  let component: EntityGridComponent;
  let fixture: ComponentFixture<EntityGridComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [
        EntityGridComponent,
        HttpClientTestingModule,
        TranslateModule.forRoot({ loader: { provide: TranslateLoader, useClass: TranslateFakeLoader } })
      ],
      providers: [
        { provide: EntityConfigurationService, useValue: { getEntityListViewConfiguration: () => of([]) } },
        { provide: Router, useValue: { navigate: jasmine.createSpy('navigate'), url: '/' } }
      ]
    })
    .compileComponents();

    fixture = TestBed.createComponent(EntityGridComponent);
    component = fixture.componentInstance;
    // Use non-empty gridData to avoid empty-state template path that uses translate with object params
    component.gridData = [{ id: 1, name: 'Test' }];
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});

