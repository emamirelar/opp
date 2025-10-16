import { Component, OnInit, signal, inject, ChangeDetectorRef, DestroyRef } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { CommonModule } from '@angular/common';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { Router } from '@angular/router';

// PrimeNG imports
import { MessageService } from 'primeng/api';
import { ToastModule } from 'primeng/toast';

// Services
import { PermissionService, EntityPermissions } from '@core/services/auth';
import { ComingSoonComponent } from '@features/static-pages/components/coming-soon/coming-soon.component';

/**
 * @uiEntity TranslationWorkbench
 * @route /admin/translations
 * @description Administrative interface for managing application translations across multiple languages. Allows Global Admins to edit, add, and maintain translation keys for the entire application.
 * @capabilities manage_translations, edit_language_files, add_translation_keys, multi_language_support
 * @synonyms translation_management, language_editor, i18n_manager, localization_workbench
 * @mandatoryFields None - Feature coming soon
 * @help_when_stuck This feature is currently under development. Only Global Admins will have access once implemented.
 * @common_tasks
 *   - Managing translations: Add, edit, or remove translation keys (Coming Soon)
 *   - Multi-language support: Edit translations for English, French, Spanish, and Portuguese (Coming Soon)
 *   - Bulk operations: Import/export translation files (Coming Soon)
 */

@Component({
  selector: 'app-translation-workbench',
  standalone: true,
  imports: [
    CommonModule,
    TranslateModule,
    ToastModule,
    ComingSoonComponent
  ],
  providers: [MessageService],
  templateUrl: './translation-workbench.component.html',
  styleUrls: ['./translation-workbench.component.scss']
})
export class TranslationWorkbenchComponent implements OnInit {
  private permissionService = inject(PermissionService);
  private messageService = inject(MessageService);
  private router = inject(Router);
  private cdr = inject(ChangeDetectorRef);
  private translateService = inject(TranslateService);
  private destroyRef = inject(DestroyRef);

  // Permission signals
  entityPermissions = signal<EntityPermissions>({
    entity: 'Translation',
    hasAccess: false,
    permissions: {
      canRead: false,
      canCreate: false,
      canUpdate: false,
      canDelete: false,
      canExport: false,
      canImport: false
    }
  });
  permissionsLoading = signal<boolean>(true);

  ngOnInit() {
    this.loadPermissions();
  }

  private loadPermissions() {
    this.permissionsLoading.set(true);
    
    // Clear cache before loading to ensure fresh permissions
    this.permissionService.clearPermissionCaches();
    
    // Get current route path for permission checking
    const currentPath = this.router.url;
    
    // Load from server (cache was cleared above)
    this.permissionService.getEntityPermissions(currentPath).pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
      next: (permissions) => {
        this.entityPermissions.set(permissions);
        this.permissionsLoading.set(false);
        
        if (!permissions.hasAccess) {
          this.messageService.add({
            severity: 'error',
            summary: this.translateService.instant('common.accessDenied'),
            detail: this.translateService.instant('common.noPermissionToAccess')
          });
          this.router.navigate(['/access-denied']);
          return;
        }
        
        this.cdr.detectChanges();
      },
      error: (error) => {
        console.error('Error loading permissions:', error);
        this.permissionsLoading.set(false);
        this.messageService.add({
          severity: 'error',
          summary: this.translateService.instant('common.error'),
          detail: this.translateService.instant('common.failedToLoadPermissions')
        });
        this.cdr.detectChanges();
      }
    });
  }
}

