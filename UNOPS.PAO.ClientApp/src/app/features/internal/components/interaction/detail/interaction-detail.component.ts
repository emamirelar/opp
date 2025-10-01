import { ChangeDetectionStrategy, Component, inject, OnInit, signal, computed, WritableSignal } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { CommonModule } from '@angular/common';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { ButtonDirective } from 'primeng/button';
import { ButtonModule } from 'primeng/button';
import { PanelModule } from 'primeng/panel';
import { TagModule } from 'primeng/tag';
import { DividerModule } from 'primeng/divider';
import { ChipModule } from 'primeng/chip';
import { SkeletonModule } from 'primeng/skeleton';
import { TooltipModule } from 'primeng/tooltip';
import { AvatarModule } from 'primeng/avatar';
import { DialogService } from 'primeng/dynamicdialog';
import { ConfirmationService } from 'primeng/api';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { DocumentComponent } from '../../../../../common/reusables/components/document/document.component';
import { GDriveDocumentComponent } from '../../../overrides/reusables/components/document/gdrive/document-gdrive.component';
import { AiPanelComponent } from '../../../../../common/reusables/components/ai-panel/ai-panel.component';

import { Interaction } from '../../../models/interaction.model';
import { InteractionService } from '../../../services/interaction.service';
import { InteractionModalComponent } from '../modal/interaction-modal.component';
import { InteractionType } from '../../../models/interaction-type.enum';
import { PermissionUtilityService } from '../../../../../essentials/services/permission-utility.service';
import { FeedbackDialogService } from '../../../../../common/reusables/services/feedback-dialog.service';
import { InteractionIconService } from '../../../../../common/services/interaction-icon.service';
import { CachedDataService } from '../../../../../common/services/cached-data.service';
import { GeminiService } from '../../../services/gemini.service';

@Component({
  selector: 'app-interaction-detail',
  standalone: true,
  imports: [
    CommonModule,
    TranslateModule,
    ButtonDirective,
    ButtonModule,
    PanelModule,
    TagModule,
    DividerModule,
    ChipModule,
    SkeletonModule,
    TooltipModule,
    AvatarModule,
    ConfirmDialogModule,
    DocumentComponent,
    GDriveDocumentComponent,
    AiPanelComponent
  ],
  providers: [DialogService, ConfirmationService],
  templateUrl: './interaction-detail.component.html',
  styleUrl: './interaction-detail.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class InteractionDetailComponent implements OnInit {
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private interactionService = inject(InteractionService);
  private dialogService = inject(DialogService);
  private permissionUtilityService = inject(PermissionUtilityService);
  private feedbackDialogService = inject(FeedbackDialogService);
  private translateService = inject(TranslateService);
  private confirmationService = inject(ConfirmationService);
  public interactionIconService = inject(InteractionIconService);
  private cachedDataService = inject(CachedDataService);
  public geminiService = inject(GeminiService);

  interaction: WritableSignal<Interaction | null> = signal(null);
  loading = signal(true);
  error = signal<string | null>(null);
  showFullDescription = signal<boolean>(false);

  // Cached data access
  allContacts = this.cachedDataService.allContacts;
  allPartners = this.cachedDataService.allPartners;
  allUsers = this.cachedDataService.allUsers;

  // Permission handling
  private permissionUtils = this.permissionUtilityService.createEntityPermissions('Interaction');
  entityPermissions = this.permissionUtils.entityPermissions;
  permissionsLoading = this.permissionUtils.permissionsLoading;

  // Computed properties for display
  interactionTypeLabel = computed(() => {
    const type = this.interaction()?.type;
    if (!type) return '';
    return this.translateService.instant(`interaction.type.${type.toLowerCase()}`);
  });

  canEdit = computed(() =>
    this.permissionUtilityService.canUpdate(this.entityPermissions())
  );

  canDelete = computed(() =>
    this.permissionUtilityService.canDelete(this.entityPermissions())
  );

  // Description display logic
  shouldShowSeeMoreButton = computed(() => {
    const description = this.interaction()?.description;
    return description && description.length > 400 && !this.showFullDescription();
  });

  shouldShowSeeLessButton = computed(() => {
    const description = this.interaction()?.description;
    return description && description.length > 400 && this.showFullDescription();
  });

  isDescriptionShort = computed(() => {
    const description = this.interaction()?.description;
    return !description || description.length <= 400;
  });

  ngOnInit() {
    this.permissionUtils.loadPermissions(this.router);
    this.loadInteraction();
  }

  private loadInteraction() {
    const id = this.route.snapshot.params['id'];
    if (!id) {
      this.error.set(this.translateService.instant('interaction.detail.error.noIdProvided'));
      this.loading.set(false);
      return;
    }

    this.loading.set(true);
    this.interactionService.getById(Number(id)).subscribe({
      next: (response) => {
        if (response.status === 404) {
          this.error.set(this.translateService.instant('interaction.detail.error.notFound', { id }));
        } else if (response.body) {
          this.interaction.set(response.body);
          this.error.set(null);
        } else {
          this.error.set(this.translateService.instant('interaction.detail.error.invalidResponse'));
        }
        this.loading.set(false);
      },
      error: (error) => {
        console.error('Error loading interaction:', error);
        const errorMessage = error.status === 404
          ? this.translateService.instant('interaction.detail.error.notFound', { id })
          : this.translateService.instant('interaction.detail.error.loadFailed', { status: error.status || this.translateService.instant('common.error.networkError') });
        this.error.set(errorMessage);
        this.loading.set(false);
      }
    });
  }

  openEditModal() {
    const currentInteraction = this.interaction();
    if (!currentInteraction || !this.canEdit()) {
      this.feedbackDialogService.showErrorToast({
        detail: this.translateService.instant('interaction.detail.error.editPermissionDenied'),
        summary: this.translateService.instant('common.error.permissionDenied')
      });
      return;
    }

    const ref = this.dialogService.open(InteractionModalComponent, {
      header: this.translateService.instant('interaction.detail.modal.editHeader'),
      closable: true,
      width: '90%',
      height: '90%',
      modal: true,
      data: {
        id: currentInteraction.id,
        initialData: currentInteraction
      }
    });

    ref.onClose.subscribe((result) => {
      if (result) {
        this.loadInteraction();
        this.feedbackDialogService.showSuccessToast({
          detail: this.translateService.instant('interaction.detail.success.updated')
        });
      }
    });
  }

  deleteInteraction() {
    const currentInteraction = this.interaction();
    if (!currentInteraction || !this.canDelete()) {
      this.feedbackDialogService.showErrorToast({
        detail: this.translateService.instant('interaction.detail.error.deletePermissionDenied'),
        summary: this.translateService.instant('common.error.permissionDenied')
      });
      return;
    }

    // Show confirmation dialog
    this.confirmationService.confirm({
      message: this.translateService.instant('interaction.detail.confirmation.deleteMessage'),
      header: this.translateService.instant('interaction.detail.confirmation.deleteHeader'),
      icon: 'pi pi-exclamation-triangle',
      accept: () => {
        this.interactionService.delete(currentInteraction.id!).subscribe({
          next: () => {
            this.feedbackDialogService.showSuccessToast({
              detail: this.translateService.instant('interaction.detail.success.deleted')
            });
            this.router.navigate(['/partnerships/interactions']);
          },
          error: (error) => {
            console.error('Error deleting interaction:', error);
            this.feedbackDialogService.showErrorToast({
              detail: this.translateService.instant('interaction.detail.error.deleteFailed')
            });
          }
        });
      }
    });
  }

  goBack() {
    // Use browser history to go back to the previous page
    window.history.back();
  }

  getInteractionIcon(type: InteractionType): string {
    return this.interactionIconService.getInteractionIcon(type);
  }

  getInteractionColor(type: InteractionType): string {
    const colors: Record<InteractionType, string> = {
      [InteractionType.Email]: 'bg-purple-500',
      [InteractionType.Chat]: 'bg-cyan-500',
      [InteractionType.Call]: 'bg-green-500',
      [InteractionType.VirtualMeeting]: 'bg-blue-500',
      [InteractionType.InPersonMeeting]: 'bg-indigo-500'
    };
    return colors[type] || 'bg-gray-500';
  }

  formatDate(date: string | Date): string {
    const d = new Date(date);
    return d.toLocaleDateString() + ' ' + d.toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' });
  }

  formatInteractionType(type: InteractionType): string {
    const typeLabels: Record<InteractionType, string> = {
      [InteractionType.Email]: 'Email',
      [InteractionType.Chat]: 'Chat',
      [InteractionType.Call]: 'Call',
      [InteractionType.VirtualMeeting]: 'Virtual Meeting',
      [InteractionType.InPersonMeeting]: 'In-Person Meeting'
    };
    return typeLabels[type] || type;
  }

  toggleDescriptionContent() {
    this.showFullDescription.set(!this.showFullDescription());
  }

  getTruncatedDescription(description: string): string {
    if (description.length <= 400) return description;

    // Find the last space before the 400 character limit to avoid cutting words
    const truncateAt = description.lastIndexOf(' ', 400);
    const cutPoint = truncateAt > 350 ? truncateAt : 400; // Fallback if no space found near limit

    return description.substring(0, cutPoint) + '...';
  }

  // Helper methods to resolve IDs to names
  getContactName(contactId: number): string {
    const contact = this.interaction()?.contacts?.find(c => Number(c.id) === contactId);
    return contact ? `${contact.firstName || ''} ${contact.lastName || ''}`.trim() || this.translateService.instant('interaction.detail.fallback.contact', { id: contactId }) : this.translateService.instant('interaction.detail.fallback.contact', { id: contactId });
  }

  getContactProfilePicture(contactId: number): string | null {
    const contact = this.interaction()?.contacts?.find(c => Number(c.id) === contactId);
    return contact?.profilePictureUrl || null;
  }

  getContactInitials(contactId: number): string {
    const contact = this.interaction()?.contacts?.find(c => Number(c.id) === contactId);
    const firstName = contact?.firstName || '';
    const lastName = contact?.lastName || '';
    const initials = `${firstName[0] || ''}${lastName[0] || ''}`.toUpperCase();
    return initials || 'C';
  }

  getPartnerName(partnerId: number): string {
    const partner = this.allPartners().find(p => p.id === partnerId);
    return partner?.name || this.translateService.instant('interaction.detail.fallback.partner', { id: partnerId });
  }

  getUserName(userId: number | undefined): string {
    if (!userId) return this.translateService.instant('interaction.detail.fallback.unknownUser');
    const user = this.allUsers().find(u => u.id === userId);
    return user?.name || this.translateService.instant('interaction.detail.fallback.user', { id: userId });
  }

  getPartnerLogo(partnerId: number): string | null {
    const partner = this.interaction()?.partners?.find(p => Number(p.id) === partnerId);
    return partner?.logoUrl || null;
  }

  getPartnerInitials(partnerId: number): string {
    const partner = this.interaction()?.partners?.find(p => Number(p.id) === partnerId);
    const name = partner?.name || partner?.partnerDescription || this.translateService.instant('interaction.detail.fallback.partner', { id: partnerId });
    return name.split(' ')
      .filter((word: string) => word.length > 0)
      .map((word: string) => word[0].toUpperCase())
      .slice(0, 2)
      .join('');
  }

  get acceptedMiMIETypesForgDrive() {
    return 'application/pdf,application/msword,application/vnd.openxmlformats-officedocument.wordprocessingml.document,application/vnd.ms-excel,application/vnd.openxmlformats-officedocument.spreadsheetml.sheet,application/vnd.google-apps.document,application/vnd.google-apps.spreadsheet';
  }

  // Navigation methods
  navigateToContact(contactId: number) {
    this.router.navigate(['/partnerships/contacts', contactId]);
  }

  navigateToPartner(partnerId: number) {
    this.router.navigate(['/partnerships/partners', partnerId]);
  }

  // AI Summary Event Handlers
  onSummaryRefresh() {
  }

  onSummaryLoaded(data: string) {
  }

  onSummaryError(error: Error) {
    console.error('AI Summary error:', error);
  }
}
