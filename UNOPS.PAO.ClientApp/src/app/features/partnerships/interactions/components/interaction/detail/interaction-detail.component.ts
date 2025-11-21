import { ChangeDetectionStrategy, Component, inject, OnInit, AfterViewInit, OnDestroy, OnChanges, SimpleChanges, signal, computed, WritableSignal, ViewChild, ElementRef, HostListener, ChangeDetectorRef } from '@angular/core';
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
import { DocumentComponent } from '@shared/components/documents/document/document.component';
import { GDriveDocumentComponent } from '@shared/components/documents/gdrive/document-gdrive.component';
import { AiPanelComponent } from '@features/ai/components/ai-panel/ai-panel.component';
import { CreateOpportunityFromInteractionsDialogComponent } from '@partnerships/interactions/components/dialogs/create-opportunity-from-interactions-dialog.component';

import { Interaction } from '@partnerships/interactions/models/interaction.model';
import { InteractionService } from '@partnerships/interactions/services/interaction.service';
import { InteractionModalComponent } from '../modal/interaction-modal.component';
import { InteractionType } from '../../../models/interaction-type.enum';
import { CreateOpportunityFromInteractionsConfig } from '../../../models/interaction-selection.model';
import { PermissionUtilityService } from '@core/services/auth';
import { FeedbackDialogService } from '@shared/services/ui';
import { InteractionIconService } from '@shared/services/domain';
import { CachedDataService } from '@shared/services/utils';
import { GeminiService } from '@ai/services/gemini.service';
import { PageContextService } from '@shared/services/utils';

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
    AiPanelComponent,
    CreateOpportunityFromInteractionsDialogComponent
  ],
  providers: [DialogService, ConfirmationService],
  templateUrl: './interaction-detail.component.html',
  styleUrl: './interaction-detail.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class InteractionDetailComponent implements OnInit, AfterViewInit, OnDestroy, OnChanges {
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
  private cdr = inject(ChangeDetectorRef);
  private pageContextService = inject(PageContextService);

  @ViewChild('widthTracker', { static: false }) widthTracker?: ElementRef;

  interaction: WritableSignal<Interaction | null> = signal(null);
  loading = signal(true);
  error = signal<string | null>(null);
  showFullDescription = signal<boolean>(false);

  // Dialog state
  showCreateOpportunityDialog = signal(false);

  // Width tracking for responsive layout
  componentWidth = signal<number>(0);
  private widthTrackingInterval?: ReturnType<typeof setInterval>;
  private resizeObserver?: ResizeObserver;

  // Computed responsive layout classes based on component width
  // Note: Interaction detail uses 60% left, 40% right layout (same as partner/contact)
  responsiveLayoutClasses = computed(() => {
    const width = this.componentWidth();
    
    // Use component width to determine layout
    // For container widths >= 700px, use side-by-side layout
    // For container widths < 700px, use stacked layout
    // Default to stacked layout when width is 0 (measuring)
    const useSideBySideLayout = width >= 700;
    
    if (useSideBySideLayout) {
      return {
        container: 'flex flex-col gap-8',
        mainLayout: 'flex flex-row gap-8 items-stretch',
        leftColumn: 'flex flex-col gap-8 w-[60%] h-full sticky top-0',
        rightColumn: 'flex flex-col gap-8 w-[40%]'
      };
    } else {
      return {
        container: 'flex flex-col gap-8',
        mainLayout: 'flex flex-col gap-8',
        leftColumn: 'flex flex-col gap-8 w-full h-full',
        rightColumn: 'flex flex-col gap-8 w-full'
      };
    }
  });

  // Debug method to check current width and layout (can be called from browser console)
  getCurrentWidth() {
    return {
      componentWidth: this.componentWidth(),
      useSideBySideLayout: this.componentWidth() >= 700,
      layoutClasses: this.responsiveLayoutClasses()
    };
  }

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

  // Computed properties for Create Opportunity dialog
  primaryPartner = computed(() => {
    const currentInteraction = this.interaction();
    if (!currentInteraction?.partners || currentInteraction.partners.length === 0) {
      return null;
    }
    // Return the first partner (or you could add logic to determine primary)
    return currentInteraction.partners[0];
  });

  canCreateOpportunity = computed(() => {
    // TODO: Add permission check when permissions are implemented
    // For now, always show the button
    return true;
    // Original logic (will be re-enabled with permissions):
    // return this.primaryPartner() !== null;
  });

  // Dialog configuration
  dialogConfig = computed<CreateOpportunityFromInteractionsConfig | null>(() => {
    const partner = this.primaryPartner();
    const currentInteraction = this.interaction();
    
    if (!currentInteraction) {
      return null;
    }
    
    // If no partner, still return config with placeholder values
    // The dialog will handle partner selection
    return {
      partnerId: partner ? Number(partner.id!) : 0,
      partnerName: partner ? (partner.name || partner.partnerDescription || 'Unknown Partner') : '',
      preSelectedInteractionIds: [currentInteraction.id!],
      currentInteractionId: currentInteraction.id,
      mode: 'detail-view'
    };
  });

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
    // Register component data for AI Assistant
    this.pageContextService.setComponentData(this);
    
    this.permissionUtils.loadPermissions(this.router);
    
    // Subscribe to route parameter changes to reload interaction when navigating
    // This fixes the issue where navigating back doesn't refresh the page data
    this.route.paramMap.subscribe(params => {
      const id = params.get('id');
      if (id) {
        this.loadInteraction(id);
      }
    });
  }

  ngAfterViewInit() {
    // Start width tracking after the view is fully initialized
    setTimeout(() => {
      this.startWidthTracking();
    }, 100);
  }

  ngOnChanges(changes: SimpleChanges): void {
    // This component gets ID from route parameters, not from @Input
    // So ngOnChanges won't trigger - route parameter subscription handles this
  }

  ngOnDestroy() {
    // Clear component data for AI Assistant
    this.pageContextService.clearComponentData();
    
    if (this.widthTrackingInterval) {
      clearInterval(this.widthTrackingInterval);
    }
    if (this.resizeObserver) {
      this.resizeObserver.disconnect();
    }
  }

  private loadInteraction(id: string) {
    if (!id) {
      this.error.set(this.translateService.instant('interaction.detail.error.noIdProvided'));
      this.loading.set(false);
      return;
    }

    this.loading.set(true);
    this.error.set(null); // Clear any previous errors
    
    this.interactionService.getById(Number(id)).subscribe({
      next: (response) => {
        if (response.status === 404) {
          this.error.set(this.translateService.instant('interaction.detail.error.notFound', { id: id }));
        } else if (response.body) {
          this.interaction.set(response.body);
          this.error.set(null);
        } else {
          this.error.set(this.translateService.instant('interaction.detail.error.invalidResponse'));
        }
        this.loading.set(false);
      },
      error: (error: any) => {
        console.error('Error loading interaction:', error);
        const errorMessage = error.status === 404
          ? this.translateService.instant('interaction.detail.error.notFound', { id: id })
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
        const id = this.route.snapshot.params['id'];
        if (id) {
          this.loadInteraction(id);
        }
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

  openCreateOpportunityDialog() {
    const currentInteraction = this.interaction();
    
    if (!currentInteraction) {
      this.feedbackDialogService.showWarningToast({
        summary: this.translateService.instant('common.warning.title'),
        detail: this.translateService.instant('message.interactionRequired')
      });
      return;
    }

    // Note: Partner is optional now - dialog will handle partner selection if needed
    this.showCreateOpportunityDialog.set(true);
  }

  handleOpportunityCreated(opportunity: any) {
    this.showCreateOpportunityDialog.set(false);
    // Navigate to the newly created opportunity (when backend is implemented)
    if (opportunity && opportunity.id) {
      this.router.navigate(['/opportunities', opportunity.id]);
    }
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
    return contact?.profilePictureUrl || 'assets/images/Contact.png';
  }

  getContactInitials(contactId: number): string {
    const contact = this.interaction()?.contacts?.find(c => Number(c.id) === contactId);
    const firstName = contact?.firstName || '';
    const lastName = contact?.lastName || '';
    const initials = `${firstName[0] || ''}${lastName[0] || ''}`.toUpperCase();
    return initials || 'C';
  }

  getPartnerName(partnerId: number): string {
    const partner = this.allPartners().find(p => p.id?.toString() === partnerId?.toString());
    return partner?.name || this.translateService.instant('interaction.detail.fallback.partner', { id: partnerId });
  }

  getUserName(userId: number | undefined): string {
    if (!userId) return this.translateService.instant('interaction.detail.fallback.unknownUser');
    const user = this.allUsers().find(u => u.id === userId);
    return user?.name || this.translateService.instant('interaction.detail.fallback.user', { id: userId });
  }

  getPartnerLogo(partnerId: number): string | null {
    const partner = this.interaction()?.partners?.find(p => Number(p.id) === partnerId);
    return partner?.logoUrl || 'assets/images/Partner.png';
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

  @HostListener('window:resize', ['$event'])
  onResize() {
    setTimeout(() => {
      this.updateComponentWidth();
    }, 10);
  }

  private startWidthTracking() {
    // Initial width measurement with multiple attempts
    this.attemptWidthMeasurement();

    // Use ResizeObserver for more efficient width tracking if available
    if (typeof ResizeObserver !== 'undefined' && this.widthTracker?.nativeElement) {
      this.resizeObserver = new ResizeObserver((entries) => {
        for (const entry of entries) {
          const width = entry.contentRect.width;
          if (width > 0) {
            const currentWidth = this.componentWidth();
            if (currentWidth !== width) {
              this.componentWidth.set(width);
              this.cdr.detectChanges();
            }
          }
        }
      });
      
      this.resizeObserver.observe(this.widthTracker.nativeElement);
    } else {
      // Fallback to polling for older browsers
      this.widthTrackingInterval = setInterval(() => {
        this.updateComponentWidth();
      }, 100);
    }
  }

  private attemptWidthMeasurement(attempts: number = 0) {
    if (attempts > 10) {
      return;
    }

    if (this.updateComponentWidth()) {
      // Width measurement successful
    } else {
      // Try again after a short delay
      setTimeout(() => {
        this.attemptWidthMeasurement(attempts + 1);
      }, 50);
    }
  }

  private updateComponentWidth(): boolean {
    if (typeof window !== 'undefined' && this.widthTracker?.nativeElement) {
      const element = this.widthTracker.nativeElement;
      const width = element.offsetWidth || element.clientWidth || 0;
      
      if (width > 0) {
        const currentWidth = this.componentWidth();
        if (currentWidth !== width) {
          this.componentWidth.set(width);
          // Trigger change detection
          this.cdr.detectChanges();
        }
        return true;
      }
    }
    return false;
  }
}
