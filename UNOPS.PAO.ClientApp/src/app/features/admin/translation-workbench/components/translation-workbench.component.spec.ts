import { ComponentFixture, TestBed } from '@angular/core/testing';
import { TranslationWorkbenchComponent } from './translation-workbench.component';
import { HttpClientTestingModule } from '@angular/common/http/testing';
import { TranslateModule, TranslateLoader, TranslateFakeLoader } from '@ngx-translate/core';
import { Router } from '@angular/router';
import { ActivatedRoute } from '@angular/router';
import { of } from 'rxjs';
import { PermissionService } from '@core/services/auth';

const mockPermissionService = jasmine.createSpyObj('PermissionService', ['getEntityPermissions', 'clearPermissionCaches']);
mockPermissionService.getEntityPermissions.and.returnValue(of({
  entity: 'Translation',
  hasAccess: false,
  permissions: { canRead: false, canCreate: false, canUpdate: false, canDelete: false, canExport: false, canImport: false }
}));
mockPermissionService.clearPermissionCaches.and.stub();

describe('TranslationWorkbenchComponent', () => {
  let component: TranslationWorkbenchComponent;
  let fixture: ComponentFixture<TranslationWorkbenchComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [
        TranslationWorkbenchComponent,
        HttpClientTestingModule,
        TranslateModule.forRoot({ loader: { provide: TranslateLoader, useClass: TranslateFakeLoader } })
      ],
      providers: [
        { provide: Router, useValue: { url: '/admin/translations', navigate: jasmine.createSpy('navigate') } },
        { provide: ActivatedRoute, useValue: { snapshot: { params: {} }, params: of({}) } },
        { provide: PermissionService, useValue: mockPermissionService }
      ]
    })
    .compileComponents();

    fixture = TestBed.createComponent(TranslationWorkbenchComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  // TODO: Add tests for translation loading
  // TODO: Add tests for translation editing
  // TODO: Add tests for translation saving
  // TODO: Add tests for language switching
  // TODO: Add tests for translation search/filter
  // TODO: Add tests for translation validation
  // TODO: Add tests for export/import functionality
});

