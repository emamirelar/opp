import { ComponentFixture, TestBed } from '@angular/core/testing';
import { EntityArtifactManagerComponent } from './entity-artifact-manager.component';
import { HttpClientTestingModule } from '@angular/common/http/testing';
import { TranslateModule } from '@ngx-translate/core';
import { Router } from '@angular/router';
import { of } from 'rxjs';
import { EntityArtifactService } from '../services/entity-artifact.service';
import { FeedbackDialogService } from '@shared/services/ui';
import { PermissionService } from '@core/services/auth';
import { MessageService } from 'primeng/api';

const mockEntityArtifactService = jasmine.createSpyObj('EntityArtifactService', ['getEntityTypes', 'getArtifactTypes', 'getEntityRecords', 'getArtifact', 'createOrUpdateArtifact']);
mockEntityArtifactService.getEntityTypes.and.returnValue(of([]));
mockEntityArtifactService.getArtifactTypes.and.returnValue(of([]));
mockEntityArtifactService.getEntityRecords.and.returnValue(of([]));
mockEntityArtifactService.getArtifact.and.returnValue(of(null));
mockEntityArtifactService.createOrUpdateArtifact.and.returnValue(of(null));

describe('EntityArtifactManagerComponent', () => {
  let component: EntityArtifactManagerComponent;
  let fixture: ComponentFixture<EntityArtifactManagerComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [
        EntityArtifactManagerComponent,
        HttpClientTestingModule,
        TranslateModule.forRoot()
      ],
      providers: [
        { provide: Router, useValue: jasmine.createSpyObj('Router', ['navigate']) },
        { provide: EntityArtifactService, useValue: mockEntityArtifactService },
        { provide: FeedbackDialogService, useValue: jasmine.createSpyObj('FeedbackDialogService', ['showConfirmDialog', 'showSuccessToast', 'showErrorToast']) },
        { provide: PermissionService, useValue: (() => {
          const ps = jasmine.createSpyObj('PermissionService', ['getEntityPermissions', 'clearPermissionCaches']);
          ps.getEntityPermissions.and.returnValue(of({ permissions: {} }));
          ps.clearPermissionCaches.and.stub();
          return ps;
        })() },
        MessageService
      ]
    })
    .compileComponents();
    
    fixture = TestBed.createComponent(EntityArtifactManagerComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});

