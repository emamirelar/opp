import { Component, computed, inject, input, OnInit, OnDestroy, output, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { PanelModule } from 'primeng/panel';
import { ButtonModule } from 'primeng/button';
import { MarkdownPipe } from '../../../../features/internal/pipes/markdown.pipe';
import { Subject, takeUntil } from 'rxjs';

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
  standalone: true,
  styles: `
    :host {
      @apply shadow-sm rounded-lg;
    }
  `
})
export class AiPanelComponent implements OnInit, OnDestroy {
  private translateService = inject(TranslateService);
  private destroy$ = new Subject<void>();

  // Inputs
  title = input.required<string>();
  entityId = input.required<string>();
  promptType = input.required<string>();
  aiService = input.required<AiDataService>();
  showRefreshButton = input<boolean>(true);
  showAiIcon = input<boolean>(true);
  loadOnInit = input<boolean>(true);
  errorMessage = input<string>('errors.failedToLoad');
  customStyles = input<string>('background-animate bg-gradient-to-r from-zinc-700 via-purple-500 to-pink-500 bg-clip-text text-transparent');
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

  // Content truncation logic
  shouldTruncate = computed(() => {
    const content = this.content();
    return content && content.length > this.truncateLength() && !this.showFullContent();
  });

  displayContent = computed(() => {
    const content = this.content();
    if (this.shouldTruncate()) {
      return content.substring(0, this.truncateLength()) + '...';
    }
    return content;
  });

  showSeeMoreButton = computed(() => {
    const content = this.content();
    return content && content.length > this.truncateLength() && !this.showFullContent();
  });

  ngOnInit() {
    if (this.loadOnInit()) {
      this.loadData();
    }
  }

  loadData() {
    if (!this.entityId() || !this.promptType() || !this.aiService()) {
      console.warn('AiPanelComponent: Missing required parameters for loading data');
      return;
    }

    this.isLoading.set(true);
    this.hasError.set(false);
    this.showFullContent.set(false); // Reset "See more" state when loading new data

    this.aiService().get(this.entityId(), this.promptType())
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (data: string) => {
          this.content.set(data);
          this.isLoading.set(false);
          this.onDataLoaded.emit(data);
        },
        error: (error: Error) => {
          console.error('AiPanelComponent error:', error);
          this.content.set(this.translateService.instant(this.errorMessage()));
          this.isLoading.set(false);
          this.hasError.set(true);
          this.onError.emit(error);
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
    this.destroy$.next();
    this.destroy$.complete();
  }
}
