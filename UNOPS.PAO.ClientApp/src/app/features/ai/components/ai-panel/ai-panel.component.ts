import {
  Component,
  computed,
  inject,
  input,
  OnInit,
  OnDestroy,
  DoCheck,
  output,
  signal,
  ChangeDetectionStrategy,
  ChangeDetectorRef,
  DestroyRef
} from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { CommonModule } from '@angular/common';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { PanelModule } from 'primeng/panel';
import { ButtonModule } from 'primeng/button';
import { MarkdownPipe } from '@shared/pipes/markdown.pipe';
import { takeUntil, Subject } from 'rxjs';

export interface AiDataService {
  get(entityId: string, promptType: string): any; // Observable<string>
}

@Component({
  selector: 'app-ai-panel',
  imports: [
    CommonModule,
    TranslateModule,
    PanelModule,
    ButtonModule,
    MarkdownPipe
  ],
  templateUrl: './ai-panel.component.html',
  styleUrls: ['./ai-panel.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush,
  standalone: true
})
export class AiPanelComponent implements OnInit, OnDestroy, DoCheck {
  private translateService = inject(TranslateService);
  private cdr = inject(ChangeDetectorRef);
  private destroyRef = inject(DestroyRef);
  private destroy$ = new Subject<void>();
  private currentAbortController: AbortController | null = null;
  private lastEntityId = '';
  private lastPromptType = '';
  private isProcessingChange = false;

  // Inputs
  title = input.required<string>();
  entityId = input.required<string>();
  promptType = input.required<string>();
  aiService = input.required<AiDataService>();
  showRefreshButton = input<boolean>(true);
  showAiIcon = input<boolean>(true);
  loadOnInit = input<boolean>(true);
  errorMessage = input<string>('errors.failedToLoad');
  customStyles = input<string>('unops-text-body-medium bg-gradient-to-r from-unops-secondary via-unops-primary to-unops-primary-light bg-clip-text text-transparent');
  truncateLength = input<number>(300); // Maximum characters to show before "See more"

  // Outputs
  onDataLoaded = output<string>();
  onError = output<Error>();
  onRefresh = output<void>();

  // Signals
  isLoading = signal<boolean>(false);
  content = signal<string>('');
  hasError = signal<boolean>(false);
  showFullContent = signal<boolean>(false);

  // Computed values
  shouldShowSpinner = computed(() => this.isLoading());
  shouldShowContent = computed(() => !this.isLoading() && !this.hasError() && this.content());
  shouldShowError = computed(() => !this.isLoading() && this.hasError());

  // Content truncation logic - now handled by CSS
  shouldTruncate = computed(() => {
    const content = this.content();
    return content && content.length > this.truncateLength() && !this.showFullContent();
  });

  showSeeMoreButton = computed(() => {
    const content = this.content();
    return content && content.length > this.truncateLength() && !this.showFullContent();
  });

  showSeeLessButton = computed(() => {
    const content = this.content();
    return content && content.length > this.truncateLength() && this.showFullContent();
  });

  toggleButtonLabel = computed(() => {
    return this.showFullContent() ? 'button.seeLess' : 'button.seeMore';
  });

  toggleButtonIcon = computed(() => {
    return this.showFullContent() ? 'pi pi-chevron-up' : 'pi pi-chevron-down';
  });

  ngOnInit() {
    if (this.loadOnInit()) {
      this.loadData();
    }
  }

  ngDoCheck() {
    // Check if parameters have changed
    const currentEntityId = this.entityId();
    const currentPromptType = this.promptType();
    
    const hasChanged = (
      currentEntityId !== this.lastEntityId || 
      currentPromptType !== this.lastPromptType
    );
    
    // If parameters changed and we're not already processing a change, reload data
    if (hasChanged && !this.isProcessingChange && currentEntityId && currentPromptType) {
      this.loadData();
    }
  }

  loadData() {
    if (!this.entityId() || !this.promptType() || !this.aiService()) {
      console.warn('AiPanelComponent: Missing required parameters for loading data');
      return;
    }

    const currentEntityId = this.entityId();
    const currentPromptType = this.promptType();

    // Set processing flag to prevent multiple simultaneous calls
    this.isProcessingChange = true;
    
    // Update tracked parameters
    this.lastEntityId = currentEntityId;
    this.lastPromptType = currentPromptType;

    // Cancel any previous request
    if (this.currentAbortController) {
      this.currentAbortController.abort();
    }
    this.currentAbortController = new AbortController();

    this.isLoading.set(true);
    this.hasError.set(false);
    this.showFullContent.set(false); // Reset "See more" state when loading new data
    this.cdr.markForCheck();

    this.aiService().get(currentEntityId, currentPromptType)
      .pipe(
        takeUntil(this.destroy$),
        takeUntilDestroyed(this.destroyRef)
      )
      .subscribe({
        next: (data: string) => {
          // Only update if this request hasn't been aborted
          if (!this.currentAbortController?.signal.aborted) {
            this.content.set(data);
            this.isLoading.set(false);
            this.isProcessingChange = false;
            this.onDataLoaded.emit(data);
            this.cdr.markForCheck();
          }
        },
        error: (error: Error) => {
          // Only handle error if this request hasn't been aborted
          if (!this.currentAbortController?.signal.aborted) {
            console.error('AiPanelComponent error:', error);
            this.content.set(this.translateService.instant(this.errorMessage()));
            this.isLoading.set(false);
            this.hasError.set(true);
            this.isProcessingChange = false;
            this.onError.emit(error);
            this.cdr.markForCheck();
          }
        }
      });
  }

  refresh() {
    this.onRefresh.emit();
    this.loadData();
  }

  toggleFullContent() {
    this.showFullContent.set(!this.showFullContent());
  }

  ngOnDestroy() {
    if (this.currentAbortController) {
      this.currentAbortController.abort();
    }
    this.destroy$.next();
    this.destroy$.complete();
  }
}
