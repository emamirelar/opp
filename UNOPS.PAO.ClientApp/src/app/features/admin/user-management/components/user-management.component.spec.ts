import { ComponentFixture, TestBed } from '@angular/core/testing';
import { UserManagementComponent } from './user-management.component';
import { HttpClientTestingModule } from '@angular/common/http/testing';
import { TranslateModule } from '@ngx-translate/core';
import { Router } from '@angular/router';
import { of } from 'rxjs';
import { UserManagementService } from '../services/user-management.service';
import { PermissionService, AuthService } from '@core/services/auth';
import { ImportDialogService } from '@features/import-export/components/import/dialog/import-dialog.service';
import { MessageService, ConfirmationService } from 'primeng/api';

const mockUserManagementService = jasmine.createSpyObj('UserManagementService', ['getUsers', 'getRoles', 'updateUser', 'getOrgUnits']);
mockUserManagementService.getUsers.and.returnValue(of({ records: [], totalCount: 0 }));
mockUserManagementService.getRoles.and.returnValue(of([]));
mockUserManagementService.getOrgUnits.and.returnValue(of([]));

describe('UserManagementComponent', () => {
  let component: UserManagementComponent;
  let fixture: ComponentFixture<UserManagementComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [
        UserManagementComponent,
        HttpClientTestingModule,
        TranslateModule.forRoot()
      ],
      providers: [
        { provide: Router, useValue: jasmine.createSpyObj('Router', ['navigate']) },
        { provide: UserManagementService, useValue: mockUserManagementService },
        { provide: PermissionService, useValue: (() => {
          const ps = jasmine.createSpyObj('PermissionService', ['getEntityPermissions', 'clearPermissionCaches']);
          ps.getEntityPermissions.and.returnValue(of({ permissions: {} }));
          ps.clearPermissionCaches.and.stub();
          return ps;
        })() },
        { provide: AuthService, useValue: jasmine.createSpyObj('AuthService', ['getCurrentUser']) },
        { provide: ImportDialogService, useValue: jasmine.createSpyObj('ImportDialogService', ['open']) },
        MessageService,
        { provide: ConfirmationService, useValue: { confirm: (opts?: { accept?: () => void }) => opts?.accept?.() } }
      ]
    })
    .compileComponents();

    fixture = TestBed.createComponent(UserManagementComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});

