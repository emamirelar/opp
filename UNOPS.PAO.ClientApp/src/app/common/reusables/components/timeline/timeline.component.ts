import { ChangeDetectionStrategy, Component, Input, ViewChild, ElementRef, OnDestroy, AfterViewInit, OnChanges, SimpleChanges, Output, EventEmitter, signal, inject, effect } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Timeline, DataSet } from 'vis-timeline/standalone';
import { HttpClient } from '@angular/common/http';
import {TranslatePipe} from '@ngx-translate/core';
import { InteractionIconService } from '../../../services/interaction-icon.service';

export interface TimelineItem {
  id: string | number;
  content: string;
  start: Date;
  type?: string;
  title?: string;
  className?: string;
  data?: any;
}

export interface TimelineConfig {
  width?: string;
  height?: string;
  margin?: {
    item?: number;
    axis?: number;
  };
  orientation?: 'top' | 'bottom';
  showCurrentTime?: boolean;
  zoomable?: boolean;
  moveable?: boolean;
  selectable?: boolean;
  multiselect?: boolean;
  tooltip?: {
    followMouse?: boolean;
    overflowMethod?: 'cap' | 'flip';
  };
  showNavigator?: boolean;
  navigatorHeight?: string;
  aggregateByDay?: boolean;
  enableClustering?: boolean;
  cluster?: {
    maxItems?: number;
    titleTemplate?: string;
    showStipes?: boolean;
    fitOnDoubleClick?: boolean;
    clusterCriteria?: (firstItem: any, secondItem: any) => boolean;
  };

  enableLazyLoading?: boolean;
  lazyLoading?: {
    bufferDays?: number;
    maxItemsPerLoad?: number;
    preloadOnZoom?: boolean;
    cacheStrategy?: 'memory' | 'session' | 'indexeddb' | 'none';
    maxCacheSize?: number;
    cacheTTL?: number;
    enablePartialLoading?: boolean;
  };

  dataLoadingStrategy?: 'full' | 'lazy' | 'navigator-full';
  navigatorRange?: {
    years?: number;
    autoLoad?: boolean;
  };
}

export interface TimelineRange {
  start: Date;
  end: Date;
  source: 'timeline' | 'navigator' | 'programmatic' | 'init';
}

export interface TimelineState {
  range: TimelineRange;
  source: 'timeline' | 'navigator' | 'programmatic' | 'init';
  isInteracting: boolean;
  lastUpdate: number;
}

export interface CachedRange {
  start: Date;
  end: Date;
  items: TimelineItem[];
  timestamp: number;
  lastAccessed: number;
  size: number;
}

export interface CacheGap {
  start: Date;
  end: Date;
}

@Component({
  selector: 'app-timeline',
  standalone: true,
  imports: [CommonModule, TranslatePipe],
  templateUrl: './timeline.component.html',
  styleUrls: ['./timeline.component.css'],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class TimelineComponent implements OnDestroy, AfterViewInit, OnChanges {
  private http = inject(HttpClient);
  private interactionIconService = inject(InteractionIconService);

  @ViewChild('timelineContainer', { static: false }) timelineContainer!: ElementRef;
  @ViewChild('navigatorContainer', { static: false }) navigatorContainer!: ElementRef;
  @ViewChild('navigatorCanvas', { static: false }) navigatorCanvas!: ElementRef<HTMLCanvasElement>;

  @Input() dataUrl?: string;
  @Input() items: TimelineItem[] = [];
  @Input() config: TimelineConfig = {};
  @Input() autoLoadFromUrl: boolean = true;
  @Input() partnerId?: number;

  @Output() itemClick = new EventEmitter<any>();
  @Output() itemSelect = new EventEmitter<any>();
  @Output() rangeChanged = new EventEmitter<{start: Date, end: Date}>();

  timeline?: Timeline;
  timelineData = new DataSet<any>([]);
  allItems: TimelineItem[] = [];
  fullDataset: TimelineItem[] = [];

  private cachedRanges: CachedRange[] = [];
  private isLoading = signal(false);
  private loadingRequests = new Set<string>();
  private cacheSize = 0;

  private navigatorAggregates: any[] = [];
  private aggregatesLoaded = false;
  private fullDataLoaded = false;

  navigatorStartDate: Date = new Date();
  navigatorEndDate: Date = new Date();
  selectionLeft: number = 0;
  selectionWidth: number = 0;
  isDragging: boolean = false;
  dragStartX: number = 0;

  private timelineState = signal<TimelineState>({
    range: {
      start: new Date(),
      end: new Date(),
      source: 'init'
    },
    source: 'init',
    isInteracting: false,
    lastUpdate: Date.now()
  });


  private updateTimelineDebounce: any;
  private navigatorSelectionDebounce: any;
  private animationFrameId: number | null = null;
  private suppressNextUpdate = false;
  private isNavigatorSelecting = false;

  private rangeChangeDebounce: any;
  private zoomDebounce: any;
  private currentLoadingRequest?: AbortController;
  private loadingState = signal<'idle' | 'debouncing' | 'loading'>('idle');
  private isLoadingGaps = false;


  private navigatorVisualUpdateDebounce: any;
  private pendingVisualUpdate: { left: number, width: number } | null = null;

  constructor() {
    effect(() => {
      const state = this.timelineState();
      this.handleStateChange(state);
    });
  }

  ngAfterViewInit() {
    this.initializeTimeline();
  }

  ngOnChanges(changes: SimpleChanges) {
    if (changes['items'] && this.timeline) {
      this.updateTimelineData(this.items);
    }
    if (changes['dataUrl'] && this.dataUrl && this.autoLoadFromUrl) {
      this.loadDataFromUrl();
    }
  }

  ngOnDestroy() {
    if (this.timeline) {
      this.timeline.destroy();
    }

    this.cleanNavigatorContainer();


    if (this.updateTimelineDebounce) {
      clearTimeout(this.updateTimelineDebounce);
    }
    if (this.navigatorSelectionDebounce) {
      clearTimeout(this.navigatorSelectionDebounce);
    }
    if (this.rangeChangeDebounce) {
      clearTimeout(this.rangeChangeDebounce);
    }
    if (this.zoomDebounce) {
      clearTimeout(this.zoomDebounce);
    }
    if (this.navigatorVisualUpdateDebounce) {
      cancelAnimationFrame(this.navigatorVisualUpdateDebounce);
    }
    if (this.animationFrameId) {
      cancelAnimationFrame(this.animationFrameId);
    }

    if (this.currentLoadingRequest) {
      this.currentLoadingRequest.abort();
    }
  }

  private initializeTimeline() {
    if (!this.timelineContainer?.nativeElement) return;

    this.initializeNavigator();

    if (this.dataUrl && this.autoLoadFromUrl) {
      this.loadUnifiedData();
    } else if (this.items.length > 0) {
      this.updateTimelineData(this.items);
      this.createTimeline();
    } else {
      this.createTimeline();
    }
  }

  private loadUnifiedData() {
    if (!this.dataUrl) return;

    const strategy = this.config.dataLoadingStrategy || 'lazy';

    if (strategy === 'full' || (strategy === 'navigator-full' && this.config.showNavigator)) {
      this.loadFullDataset();
    } else {
      this.loadDataFromUrl();
    }
  }

  private loadFullDataset() {
    if (!this.dataUrl || this.fullDataLoaded) return;

    const fullDataUrl = this.buildFullDataUrl();
    console.log('Loading full dataset for navigator:', fullDataUrl);

    this.isLoading.set(true);
    this.http.get<any>(fullDataUrl).subscribe({
      next: (response: any) => {
        const records = response.records || response;
        if (Array.isArray(records)) {
          const timelineItems = records.map((record: any) => this.convertToTimelineItem(record));

          this.fullDataset = timelineItems;
          this.fullDataLoaded = true;

          this.generateNavigatorAggregatesFromFullData();

          if (this.config.enableLazyLoading) {
            this.initializeLazyLoadingFromFullData();
          } else {
            this.allItems = timelineItems;
            this.updateTimelineData(timelineItems);
          }

          this.createTimeline();
        }
        this.isLoading.set(false);
      },
      error: (error: any) => {
        console.error('Failed to load full dataset:', error);
        this.isLoading.set(false);
        this.loadDataFromUrl();
      }
    });
  }

  private loadDataFromUrl() {
    if (!this.dataUrl) return;

    this.http.get<any>(this.dataUrl).subscribe({
      next: (response: any) => {
        const records = response.records || response;
        if (Array.isArray(records)) {
          const timelineItems = records.map((record: any) => this.convertToTimelineItem(record));
          this.allItems = timelineItems;
          this.updateTimelineData(timelineItems);
          this.createTimeline();
        }
      },
      error: (error: any) => {
        console.error('Failed to load timeline data:', error);
      }
    });
  }

  private buildFullDataUrl(): string {
    if (!this.dataUrl) return '';

    const separator = this.dataUrl.includes('?') ? '&' : '?';
    const yearsBack = this.config.navigatorRange?.years || 3;
    const fromDate = new Date();
    fromDate.setFullYear(fromDate.getFullYear() - yearsBack);

    const fromDateParam = encodeURIComponent(fromDate.toISOString().split('T')[0]);
    const toDateParam = encodeURIComponent(new Date().toISOString().split('T')[0]);

    return `${this.dataUrl}${separator}fromDate=${fromDateParam}&toDate=${toDateParam}&pageSize=2000`;
  }

  private generateNavigatorAggregatesFromFullData() {
    if (!this.fullDataset.length) return;

    const dayGroups = new Map<string, TimelineItem[]>();

    this.fullDataset.forEach(item => {
      const dayKey = new Date(item.start).toISOString().split('T')[0];
      if (!dayGroups.has(dayKey)) {
        dayGroups.set(dayKey, []);
      }
      dayGroups.get(dayKey)!.push(item);
    });

    this.navigatorAggregates = Array.from(dayGroups.entries()).map(([date, items]) => ({
      date,
      count: items.length,
      items
    }));

    this.aggregatesLoaded = true;
    console.log(`Generated ${this.navigatorAggregates.length} navigator aggregates from ${this.fullDataset.length} items`);
  }

  private initializeLazyLoadingFromFullData() {
    const bufferDays = this.config.lazyLoading?.bufferDays || 30;
    const now = new Date();
    const initialStart = new Date(now.getTime() - (bufferDays * 24 * 60 * 60 * 1000));
    const initialEnd = new Date(now.getTime() + (bufferDays * 24 * 60 * 60 * 1000));

    const initialItems = this.getItemsFromFullDataset(initialStart, initialEnd);
    this.allItems = initialItems;
    this.updateTimelineData(initialItems);

    const cacheRange: CachedRange = {
      start: initialStart,
      end: initialEnd,
      items: initialItems,
      timestamp: Date.now(),
      lastAccessed: Date.now(),
      size: this.estimateSize(initialItems)
    };

    this.cachedRanges = [cacheRange];
    this.cacheSize = cacheRange.size;
  }

  private getItemsFromFullDataset(start: Date, end: Date): TimelineItem[] {
    if (!this.fullDataset.length) return [];

    return this.fullDataset.filter(item => {
      const itemTime = item.start.getTime();
      return itemTime >= start.getTime() && itemTime <= end.getTime();
    });
  }

  private convertToTimelineItem(record: any): TimelineItem {
    return {
      id: record.id,
      content: this.createTimelineItemContent(record),
      start: new Date(record.date),
      type: 'point',
      title: `${record.type || 'Unknown'}: ${record.subject || 'No subject'}`,
      className: this.getTimelineItemClass(record.type),
      data: record
    };
  }

  private createTimelineItemContent(record: any): string {
    const interactionType = record?.type || 'other';
    const iconInfo = this.interactionIconService.getInteractionIconInfo(interactionType);
    const unicodeIcon = this.getSimpleUnicodeIcon(interactionType);

    return `
      <div style="display: flex; align-items: center; gap: 6px; padding: 2px;">
        <span style="font-size: 14px; color: ${iconInfo.color}; font-weight: bold; line-height: 1;">${unicodeIcon}</span>
        <span style="font-size: 11px; color: #374151; font-weight: 500; white-space: nowrap;">${record.contactName || 'No contact'}</span>
      </div>
    `;
  }

  private getSimpleUnicodeIcon(type: string | null | undefined): string {
    const safeType = (type && typeof type === 'string') ? type.toLowerCase() : 'other';

    const iconMap: { [key: string]: string } = {
      'email': '✉️',
      'phone': '📞',
      'call': '📞',
      'chat': '💭',
      'virtualmeetingg': '🎥',
      'video call': '🎥',
      'inpersonmeeting': '🤝',
      'meeting': '🤝',
      'note': '📄',
      'task': '✅',
      'appointment': '🗓️',
      'other': '⚪'
    };
    return iconMap[safeType] || iconMap['other'];
  }

  private getTimelineItemClass(type: string): string {
    const safeType = (type && typeof type === 'string') ? type.toLowerCase() : 'other';

    const typeClasses: { [key: string]: string } = {
      'email': 'timeline-email',
      'phone': 'timeline-phone',
      'call': 'timeline-phone',
      'chat': 'timeline-chat',
      'virtualmeetingg': 'timeline-video',
      'video call': 'timeline-video',
      'inpersonmeeting': 'timeline-meeting',
      'meeting': 'timeline-meeting',
      'note': 'timeline-note',
      'task': 'timeline-task',
      'appointment': 'timeline-appointment',
      'other': 'timeline-other'
    };
    return typeClasses[safeType] || 'timeline-other';
  }

  private updateTimelineData(items: TimelineItem[]) {
    this.allItems = items;
    this.timelineData.clear();
    this.timelineData.add(items);
  }

  private createTimeline() {
    if (!this.timelineContainer?.nativeElement) {
      console.error('Timeline container not found');
      return;
    }


    this.timelineContainer.nativeElement.innerHTML = '';

    const defaultConfig: TimelineConfig = {
      width: '100%',
      height: '200px',
      margin: { item: 5, axis: 20 },
      orientation: 'top',
      showCurrentTime: true,
      zoomable: true,
      moveable: true,
      selectable: true,
      multiselect: false,
      tooltip: {
        followMouse: true,
        overflowMethod: 'cap'
      }
    };


    const {
      showNavigator,
      navigatorHeight,
      aggregateByDay,
      enableClustering,
      cluster: clusterConfig,
      enableLazyLoading,
      lazyLoading,
      ...visTimelineConfig
    } = this.config;

    const options = {
      ...defaultConfig,
      ...visTimelineConfig,
      format: {
        minorLabels: {
          millisecond: 'SSS',
          second: 's',
          minute: 'HH:mm',
          hour: 'HH:mm',
          weekday: 'ddd D',
          day: 'D',
          week: 'w',
          month: 'MMM',
          year: 'YYYY'
        },
        majorLabels: {
          millisecond: 'HH:mm:ss',
          second: 'D MMMM HH:mm',
          minute: 'ddd D MMMM',
          hour: 'ddd D MMMM',
          weekday: 'MMMM YYYY',
          day: 'MMMM YYYY',
          week: 'MMMM YYYY',
          month: 'YYYY',
          year: ''
        }
      },

      ...(enableClustering && {
        cluster: {
          maxItems: clusterConfig?.maxItems || 3,
          titleTemplate: clusterConfig?.titleTemplate || 'Groupe de {count} interactions',
          showStipes: clusterConfig?.showStipes ?? true,
          fitOnDoubleClick: clusterConfig?.fitOnDoubleClick ?? true,
          clusterCriteria: clusterConfig?.clusterCriteria || this.defaultClusterCriteria.bind(this)
        }
      })
    };


    this.timeline = new Timeline(this.timelineContainer.nativeElement, this.timelineData, options);

    this.timeline.on('select', (event) => {
      if (event.items.length > 0) {
        const itemId = event.items[0];
        const item = this.timelineData.get(itemId) as any;
        if (item) {
          this.itemSelect.emit(item.data || item);
        }
      }
    });

    this.timeline.on('click', (event) => {
      if (event.item) {
        const item = this.timelineData.get(event.item) as any;
        if (item) {
          this.itemClick.emit(item.data || item);
        }
      }
    });


    this.timeline.on('rangechange', () => {
      if (!this.suppressNextUpdate) {
        this.updateTimelineState('timeline');

        if (this.config.enableLazyLoading) {
          this.debounceDataLoad('navigation', 300);
        }
      }
    });

    this.timeline.on('rangechanged', () => {
      if (!this.suppressNextUpdate) {
        this.updateTimelineState('timeline');

        if (this.config.enableLazyLoading) {
          this.debounceDataLoad('final', 200);
        }
      }
    });


    this.timeline.on('zoom', () => {
      if (!this.suppressNextUpdate && this.config.enableLazyLoading) {
        this.debounceDataLoad('zoom', 500);
      }
    });


    this.timeline.on('mouseDown', () => {
      this.setInteractionState(true);
    });

    this.timeline.on('mouseUp', () => {
      this.setInteractionState(false);
    });


    setTimeout(() => {
      if (this.timeline) {
        this.timeline.fit();
      }

      if (this.config.showNavigator) {
        setTimeout(() => this.drawNavigator(), 50);
      }
    }, 100);
  }

  private initializeNavigator() {

    const now = new Date();
    this.navigatorEndDate = new Date(now.getFullYear(), now.getMonth(), now.getDate());
    this.navigatorStartDate = new Date(now.getFullYear() - 3, now.getMonth(), now.getDate());


    this.loadNavigatorAggregates();
  }

  private loadNavigatorAggregates() {
    if (!this.config.showNavigator || this.aggregatesLoaded) return;

    if (this.fullDataLoaded) {
      this.generateNavigatorAggregatesFromFullData();
    } else {
      this.aggregatesLoaded = true;
    }

    if (this.navigatorCanvas?.nativeElement) {
      this.drawNavigator();
    }
  }

  private drawNavigator() {
    if (!this.config.showNavigator || !this.navigatorCanvas?.nativeElement) return;

    const canvas = this.navigatorCanvas.nativeElement;
    const ctx = canvas.getContext('2d');
    if (!ctx) return;


    this.cleanNavigatorContainer();


    const rect = canvas.getBoundingClientRect();
    canvas.width = rect.width;
    canvas.height = rect.height;


    ctx.clearRect(0, 0, canvas.width, canvas.height);


    this.drawNavigatorGrid(ctx, canvas.width, canvas.height);


    this.drawNavigatorData(ctx, canvas.width, canvas.height);


    this.updateNavigatorFromCurrentTimeline();
  }

  private cleanNavigatorContainer() {
    if (!this.navigatorContainer?.nativeElement) return;


    const visElements = this.navigatorContainer.nativeElement.querySelectorAll('.vis-timeline');
    visElements.forEach((element: Element) => {
      element.remove();
    });
  }

  private drawNavigatorGrid(ctx: CanvasRenderingContext2D, width: number, height: number) {
    const totalDuration = this.navigatorEndDate.getTime() - this.navigatorStartDate.getTime();
    const currentYear = this.navigatorStartDate.getFullYear();

    ctx.strokeStyle = '#e0e0e0';
    ctx.lineWidth = 1;
    ctx.font = '10px Arial';
    ctx.fillStyle = '#666';


    for (let year = currentYear; year <= this.navigatorEndDate.getFullYear(); year++) {
      const yearStart = new Date(year, 0, 1).getTime();
      const x = ((yearStart - this.navigatorStartDate.getTime()) / totalDuration) * width;

      if (x >= 0 && x <= width) {

        ctx.beginPath();
        ctx.moveTo(x, 0);
        ctx.lineTo(x, height);
        ctx.stroke();


        ctx.fillText(year.toString(), x + 2, 12);
      }
    }
  }

  private drawNavigatorData(ctx: CanvasRenderingContext2D, width: number, height: number) {
    const totalDuration = this.navigatorEndDate.getTime() - this.navigatorStartDate.getTime();


    if (this.navigatorAggregates.length > 0) {
      this.drawAggregateNavigatorData(ctx, width, height, totalDuration);
    } else if (this.allItems.length > 0) {
      if (this.config.aggregateByDay) {
        this.drawAggregatedData(ctx, width, height, totalDuration);
      } else {
        this.drawSimpleData(ctx, width, height, totalDuration);
      }
    }
  }

  private drawAggregateNavigatorData(ctx: CanvasRenderingContext2D, width: number, height: number, totalDuration: number) {

    const maxCount = Math.max(...this.navigatorAggregates.map(a => a.count));
    if (maxCount === 0) return;

    this.navigatorAggregates.forEach(aggregate => {
      const date = new Date(aggregate.date).getTime();

      if (date >= this.navigatorStartDate.getTime() && date <= this.navigatorEndDate.getTime()) {
        const x = ((date - this.navigatorStartDate.getTime()) / totalDuration) * width;


        const normalizedHeight = Math.max(3, (aggregate.count / maxCount) * 12);


        const intensity = aggregate.count / maxCount;
        ctx.fillStyle = this.getIntensityColor(intensity);


        ctx.fillRect(x - 1, height - normalizedHeight, 2, normalizedHeight);


        if (aggregate.count > 0) {
          ctx.fillStyle = ctx.fillStyle;
          ctx.beginPath();
          ctx.arc(x, height - normalizedHeight - 2, 1, 0, 2 * Math.PI);
          ctx.fill();
        }
      }
    });
  }

  /**
   * Génère une couleur en dégradé d'une seule teinte basée sur l'intensité (0-1)
   * Dégradé réduit pour une meilleure visibilité des faibles interactions
   */
  private getIntensityColor(intensity: number): string {
    const normalizedIntensity = Math.max(0, Math.min(1, intensity));

    if (normalizedIntensity === 0) {
      return 'rgba(33, 150, 243, 0.3)';
    }

    const baseR = 33;
    const baseG = 150;
    const baseB = 243;

    // Réduction du dégradé : opacité entre 0.5 et 0.9 au lieu de 0.2 et 1.0
    const minOpacity = 0.5;
    const maxOpacity = 0.9;
    const opacity = minOpacity + (maxOpacity - minOpacity) * normalizedIntensity;

    // Réduction du facteur d'assombrissement : 0.1 au lieu de 0.3
    const darkenFactor = 1 - (normalizedIntensity * 0.1);
    const r = Math.round(baseR * darkenFactor);
    const g = Math.round(baseG * darkenFactor);
    const b = Math.round(baseB * darkenFactor);

    return `rgba(${r}, ${g}, ${b}, ${opacity})`;
  }

  private drawAggregatedData(ctx: CanvasRenderingContext2D, width: number, height: number, totalDuration: number) {

    const dayGroups = new Map<string, TimelineItem[]>();

    this.allItems.forEach(item => {
      const dayKey = new Date(item.start).toISOString().split('T')[0];
      if (!dayGroups.has(dayKey)) {
        dayGroups.set(dayKey, []);
      }
      dayGroups.get(dayKey)!.push(item);
    });


    dayGroups.forEach((items, dayKey) => {
      const date = new Date(dayKey).getTime();
      if (date >= this.navigatorStartDate.getTime() && date <= this.navigatorEndDate.getTime()) {
        const x = ((date - this.navigatorStartDate.getTime()) / totalDuration) * width;
        const radius = Math.min(8, Math.max(3, items.length * 2));

        ctx.fillStyle = items.length > 3 ? '#4caf50' : '#2196f3';
        ctx.beginPath();
        ctx.arc(x, height / 2, radius, 0, 2 * Math.PI);
        ctx.fill();


        if (items.length > 1) {
          ctx.fillStyle = 'white';
          ctx.font = '8px Arial';
          ctx.textAlign = 'center';
          ctx.fillText(items.length.toString(), x, height / 2 + 2);
        }
      }
    });
  }

  private drawSimpleData(ctx: CanvasRenderingContext2D, width: number, height: number, totalDuration: number) {
    this.allItems.forEach(item => {
      const date = item.start.getTime();
      if (date >= this.navigatorStartDate.getTime() && date <= this.navigatorEndDate.getTime()) {
        const x = ((date - this.navigatorStartDate.getTime()) / totalDuration) * width;

        ctx.fillStyle = '#2196f3';
        ctx.beginPath();
        ctx.arc(x, height / 2, 3, 0, 2 * Math.PI);
        ctx.fill();
      }
    });
  }

  private animateSelectionTo(targetLeft: number, targetWidth: number) {
    const duration = 150;
    const startTime = performance.now();
    const startLeft = this.selectionLeft;
    const startWidth = this.selectionWidth;

    const animate = (currentTime: number) => {
      const elapsed = currentTime - startTime;
      const progress = Math.min(elapsed / duration, 1);


      const easeProgress = this.easeOutCubic(progress);

      this.selectionLeft = startLeft + (targetLeft - startLeft) * easeProgress;
      this.selectionWidth = startWidth + (targetWidth - startWidth) * easeProgress;

      if (progress < 1) {
        requestAnimationFrame(animate);
      }
    };

    requestAnimationFrame(animate);
  }

  private easeOutCubic(t: number): number {
    return 1 - Math.pow(1 - t, 3);
  }



  private handleStateChange(state: TimelineState) {
    if (!this.timeline || !this.navigatorContainer?.nativeElement) return;

    const { range, source } = state;

    if (this.isNavigatorSelecting && source !== 'navigator') {
      return;
    }

    this.suppressNextUpdate = true;

    try {
      if (source === 'navigator') {
        this.updateTimelineFromState(range, true);
      } else if (source === 'timeline') {
        if (!this.isNavigatorSelecting) {
          this.updateNavigatorFromState(range);
        }
      } else if (source === 'programmatic') {
        this.updateTimelineFromState(range, false);
        if (!this.isNavigatorSelecting) {
          this.updateNavigatorFromState(range);
        }
      }

      this.rangeChanged.emit({
        start: range.start,
        end: range.end
      });

    } finally {
      setTimeout(() => {
        this.suppressNextUpdate = false;
      }, 10);
    }
  }

  private updateTimelineState(source: TimelineRange['source'], customRange?: { start: Date, end: Date }) {
    if (this.suppressNextUpdate) return;

    let range: TimelineRange;

    if (customRange) {
      range = { ...customRange, source };
    } else if (this.timeline) {
      const window = this.timeline.getWindow();
      range = {
        start: new Date(window.start),
        end: new Date(window.end),
        source
      };
    } else {
      return;
    }

    this.timelineState.set({
      range,
      source,
      isInteracting: this.isDragging || this.timelineState().isInteracting,
      lastUpdate: Date.now()
    });
  }

  private updateTimelineFromState(range: TimelineRange, withAnimation: boolean) {
    if (!this.timeline) return;

    const options = withAnimation
      ? { animation: { duration: 300 } }
      : { animation: false };

    this.timeline.setWindow(range.start, range.end, options);
  }

  private updateNavigatorFromState(range: TimelineRange) {
    if (!this.navigatorContainer?.nativeElement) return;

    const containerWidth = this.navigatorContainer.nativeElement.offsetWidth;
    const totalDuration = this.navigatorEndDate.getTime() - this.navigatorStartDate.getTime();

    const selectionStart = Math.max(range.start.getTime(), this.navigatorStartDate.getTime());
    const selectionEnd = Math.min(range.end.getTime(), this.navigatorEndDate.getTime());

    const newLeft = ((selectionStart - this.navigatorStartDate.getTime()) / totalDuration) * containerWidth;
    const newWidth = ((selectionEnd - selectionStart) / totalDuration) * containerWidth;


    if (this.timelineState().isInteracting) {

      this.selectionLeft = newLeft;
      this.selectionWidth = newWidth;
    } else {

      this.animateSelectionTo(newLeft, newWidth);
    }
  }

  private setInteractionState(isInteracting: boolean) {
    const currentState = this.timelineState();
    this.timelineState.set({
      ...currentState,
      isInteracting,
      lastUpdate: Date.now()
    });
  }


  onNavigatorMouseDown(event: MouseEvent) {
    this.isDragging = true;
    this.isNavigatorSelecting = true;
    this.dragStartX = event.offsetX;
    this.selectionLeft = event.offsetX;
    this.selectionWidth = 0;


    if (this.navigatorSelectionDebounce) {
      clearTimeout(this.navigatorSelectionDebounce);
    }
  }

  onNavigatorMouseMove(event: MouseEvent) {
    if (!this.isDragging) return;

    const currentX = event.offsetX;
    const startX = Math.min(this.dragStartX, currentX);
    const endX = Math.max(this.dragStartX, currentX);


    this.pendingVisualUpdate = {
      left: startX,
      width: endX - startX
    };


    this.debounceNavigatorVisualUpdate();
  }

  onNavigatorMouseUp(event: MouseEvent) {
    if (!this.isDragging) return;

    if (this.pendingVisualUpdate) {
      this.selectionLeft = this.pendingVisualUpdate.left;
      this.selectionWidth = this.pendingVisualUpdate.width;
      this.pendingVisualUpdate = null;
    }

    this.isDragging = false;

    if (this.selectionWidth > 10) {
      // Sélection par glissé-déposé normale
      this.debouncedApplyNavigatorSelection();
    } else {
      // Simple clic : créer une sélection d'un mois centré sur le point cliqué
      this.createMonthSelectionAroundClick(event.offsetX);
    }
  }

  private createMonthSelectionAroundClick(clickX: number) {
    if (!this.navigatorContainer?.nativeElement) return;

    const containerWidth = this.navigatorContainer.nativeElement.offsetWidth;
    const totalDuration = this.navigatorEndDate.getTime() - this.navigatorStartDate.getTime();

    // Convertir la position de clic en date
    const clickRatio = clickX / containerWidth;
    const clickDate = new Date(this.navigatorStartDate.getTime() + (clickRatio * totalDuration));

    // Créer une sélection d'un mois (30 jours) centrée sur le clic
    const halfMonth = 15 * 24 * 60 * 60 * 1000; // 15 jours en millisecondes
    const selectionStart = new Date(clickDate.getTime() - halfMonth);
    const selectionEnd = new Date(clickDate.getTime() + halfMonth);

    // S'assurer que la sélection ne dépasse pas les limites du navigator
    const clampedStart = new Date(Math.max(selectionStart.getTime(), this.navigatorStartDate.getTime()));
    const clampedEnd = new Date(Math.min(selectionEnd.getTime(), this.navigatorEndDate.getTime()));

    // Calculer les positions visuelles de la sélection
    const startRatio = (clampedStart.getTime() - this.navigatorStartDate.getTime()) / totalDuration;
    const endRatio = (clampedEnd.getTime() - this.navigatorStartDate.getTime()) / totalDuration;

    this.selectionLeft = startRatio * containerWidth;
    this.selectionWidth = (endRatio - startRatio) * containerWidth;

    // Appliquer la sélection
    this.debouncedApplyNavigatorSelection();
  }

  private debouncedApplyNavigatorSelection() {
    if (this.navigatorSelectionDebounce) {
      clearTimeout(this.navigatorSelectionDebounce);
    }

    this.navigatorSelectionDebounce = setTimeout(() => {
      this.applyNavigatorSelection();

      setTimeout(() => {
        this.isNavigatorSelecting = false;
      }, 100);
    }, 250);
  }

  private debounceNavigatorVisualUpdate() {

    if (this.navigatorVisualUpdateDebounce) {
      cancelAnimationFrame(this.navigatorVisualUpdateDebounce);
    }


    this.navigatorVisualUpdateDebounce = requestAnimationFrame(() => {
      if (this.pendingVisualUpdate && this.isDragging) {
        this.selectionLeft = this.pendingVisualUpdate.left;
        this.selectionWidth = this.pendingVisualUpdate.width;
        this.pendingVisualUpdate = null;
      }
    });
  }

  private applyNavigatorSelection() {
    if (!this.navigatorContainer?.nativeElement) return;

    const containerWidth = this.navigatorContainer.nativeElement.offsetWidth;
    const totalDuration = this.navigatorEndDate.getTime() - this.navigatorStartDate.getTime();

    const startRatio = this.selectionLeft / containerWidth;
    const endRatio = (this.selectionLeft + this.selectionWidth) / containerWidth;

    const selectedStart = new Date(this.navigatorStartDate.getTime() + (startRatio * totalDuration));
    const selectedEnd = new Date(this.navigatorStartDate.getTime() + (endRatio * totalDuration));


    if (this.config.enableLazyLoading) {
      this.immediateDataLoad(selectedStart, selectedEnd);
    }



    if (this.timeline) {
      this.suppressNextUpdate = true;

      this.timeline.setWindow(selectedStart, selectedEnd, {
        animation: { duration: 300 }
      });


      this.rangeChanged.emit({
        start: selectedStart,
        end: selectedEnd
      });

      setTimeout(() => {
        this.suppressNextUpdate = false;
      }, 350);
    }
  }

  resetNavigatorView() {
    if (this.timeline) {
      this.timeline.fit();

    }
  }

  getYearRange(): string {
    return `${this.navigatorStartDate.getFullYear()} - ${this.navigatorEndDate.getFullYear()}`;
  }

  public refreshTimeline() {
    if (this.dataUrl && this.autoLoadFromUrl) {
      this.fullDataLoaded = false;
      this.aggregatesLoaded = false;
      this.cachedRanges = [];
      this.cacheSize = 0;
      this.loadUnifiedData();
    } else if (this.timeline) {
      this.timeline.redraw();
      if (this.config.showNavigator) {
        this.drawNavigator();
      }
    }
  }

  public fitTimeline() {
    if (this.timeline) {
      this.timeline.fit();

    }
  }

  public setTimelineRange(start: Date, end: Date) {

    this.updateTimelineState('programmatic', { start, end });
  }

  public getTimelineRange(): { start: Date, end: Date } | null {
    if (this.timeline) {
      const window = this.timeline.getWindow();
      return {
        start: new Date(window.start),
        end: new Date(window.end)
      };
    }
    return null;
  }

  private updateNavigatorFromCurrentTimeline() {
    if (!this.timeline) return;

    const window = this.timeline.getWindow();
    const range: TimelineRange = {
      start: new Date(window.start),
      end: new Date(window.end),
      source: 'init'
    };

    this.updateNavigatorFromState(range);
  }



  private defaultClusterCriteria(firstItem: any, secondItem: any): boolean {
    if (!this.timeline) return false;


    const window = this.timeline.getWindow();
    const rangeDuration = window.end.getTime() - window.start.getTime();
    const rangeDays = rangeDuration / (1000 * 60 * 60 * 24);

    const timeDifference = Math.abs(firstItem.start.getTime() - secondItem.start.getTime());
    let maxTimeDifference: number;


    if (rangeDays > 365) {

      maxTimeDifference = 90 * 24 * 60 * 60 * 1000;
    } else if (rangeDays > 180) {

      maxTimeDifference = 30 * 24 * 60 * 60 * 1000;
    } else if (rangeDays > 60) {

      maxTimeDifference = 7 * 24 * 60 * 60 * 1000;
    } else {

      maxTimeDifference = 24 * 60 * 60 * 1000;
    }

    return timeDifference <= maxTimeDifference;
  }



  /**
   * Debounce le chargement des données selon le type d'interaction
   */
  private debounceDataLoad(type: 'navigation' | 'zoom' | 'final', delay: number): void {

    if (type === 'navigation' || type === 'final') {
      if (this.rangeChangeDebounce) {
        clearTimeout(this.rangeChangeDebounce);
      }
    }
    if (type === 'zoom') {
      if (this.zoomDebounce) {
        clearTimeout(this.zoomDebounce);
      }
    }


    this.loadingState.set('debouncing');

    const debounceHandler = () => {
      this.checkAndLoadVisibleRange().then(() => {

        if (type === 'final' && this.config.lazyLoading?.preloadOnZoom) {
          this.preloadAdjacentRanges();
        }
      });
    };


    if (type === 'navigation' || type === 'final') {
      this.rangeChangeDebounce = setTimeout(debounceHandler, delay);
    } else if (type === 'zoom') {
      this.zoomDebounce = setTimeout(debounceHandler, delay);
    }
  }

  /**
   * Chargement immédiat pour les sélections navigator (pas de debounce)
   */
  private immediateDataLoad(start: Date, end: Date): void {
    this.checkAndLoadVisibleRange(start, end);
  }

  private async loadDataForRange(start: Date, end: Date, forceReload = false): Promise<void> {
    if (!this.config.enableLazyLoading || !this.dataUrl) {
      return this.loadUnifiedData();
    }

    if (this.fullDataLoaded) {
      const items = this.getItemsFromFullDataset(start, end);
      this.mergeTimelineData(items);
      this.loadingState.set('idle');
      return;
    }

    if (this.cachedRanges.length === 0) {
      await this.loadCacheFromStorage();
    }

    const bufferDays = this.config.lazyLoading?.bufferDays || 30;
    const bufferedStart = new Date(start.getTime() - (bufferDays * 24 * 60 * 60 * 1000));
    const bufferedEnd = new Date(end.getTime() + (bufferDays * 24 * 60 * 60 * 1000));

    if (!forceReload) {
      const coverage = this.getCacheCoverage(bufferedStart, bufferedEnd);

      if (coverage.isFullyCovered) {
        const cachedItems = this.getItemsFromCache(start, end);
        this.mergeTimelineData(cachedItems);
        this.loadingState.set('idle');
        return;
      }

      if (this.config.lazyLoading?.enablePartialLoading && coverage.gaps.length > 0 && !this.isLoadingGaps) {
        await this.loadGaps(coverage.gaps);
        return;
      }
    }

    const rangeKey = this.getRangeKey(bufferedStart, bufferedEnd);

    if (this.loadingRequests.has(rangeKey)) {
      return;
    }

    if (this.currentLoadingRequest) {
      this.currentLoadingRequest.abort();
    }

    this.loadingRequests.add(rangeKey);
    this.isLoading.set(true);
    this.loadingState.set('loading');

    this.currentLoadingRequest = new AbortController();

    try {
      const url = this.buildLazyLoadUrl(bufferedStart, bufferedEnd);
      console.log('Loading main data from URL:', url);

      const response = await fetch(url, {
        signal: this.currentLoadingRequest.signal
      });

      if (!response.ok) {
        console.error('Main load failed:', {
          url,
          status: response.status,
          statusText: response.statusText,
          bufferedStart: bufferedStart.toISOString(),
          bufferedEnd: bufferedEnd.toISOString()
        });
        throw new Error(`HTTP error! status: ${response.status}`);
      }

      const data = await response.json();
      const records = data.records || data;

      if (Array.isArray(records)) {
        const timelineItems = records.map((record: any) => this.convertToTimelineItem(record));

        await this.addToCache(bufferedStart, bufferedEnd, timelineItems);

        this.mergeTimelineData(timelineItems);

        if (this.config.showNavigator) {
          this.drawNavigator();
        }
      }
    } catch (error: any) {
      if (error.name !== 'AbortError') {
        console.error('Failed to load timeline data for range:', error);
      }
    } finally {
      this.loadingRequests.delete(rangeKey);
      this.isLoading.set(false);
      this.loadingState.set('idle');
      this.currentLoadingRequest = undefined;
    }
  }

  private buildLazyLoadUrl(start: Date, end: Date): string {
    if (!this.dataUrl) return '';

    const separator = this.dataUrl.includes('?') ? '&' : '?';


    const fromDateParam = encodeURIComponent(start.toISOString().split('T')[0]);
    const toDateParam = encodeURIComponent(end.toISOString().split('T')[0]);
    const maxItems = this.config.lazyLoading?.maxItemsPerLoad || 1000;


    return `${this.dataUrl}${separator}fromDate=${fromDateParam}&toDate=${toDateParam}&pageSize=${maxItems}&pageIndex=1`;
  }

  private getRangeKey(start: Date, end: Date): string {
    return `${start.toISOString()}_${end.toISOString()}`;
  }

  private mergeTimelineData(newItems: TimelineItem[]): void {

    const existingIds = new Set(this.allItems.map(item => item.id));


    const uniqueNewItems = newItems.filter(item => !existingIds.has(item.id));

    if (uniqueNewItems.length > 0) {
      this.allItems = [...this.allItems, ...uniqueNewItems];
      this.timelineData.add(uniqueNewItems);
    }
  }

  private async checkAndLoadVisibleRange(customStart?: Date, customEnd?: Date): Promise<void> {
    if (!this.config.enableLazyLoading) return;

    let viewStart: Date, viewEnd: Date;

    if (customStart && customEnd) {

      viewStart = customStart;
      viewEnd = customEnd;
    } else if (this.timeline) {

      const window = this.timeline.getWindow();
      viewStart = new Date(window.start);
      viewEnd = new Date(window.end);
    } else {
      return;
    }


    const coverage = this.getCacheCoverage(viewStart, viewEnd);

    if (!coverage.isFullyCovered) {
      await this.loadDataForRange(viewStart, viewEnd);
    }
  }



  /**
   * Vérifier la couverture du cache pour une plage donnée
   */
  private getCacheCoverage(start: Date, end: Date): { isFullyCovered: boolean, gaps: CacheGap[] } {

    this.cleanExpiredCache();


    this.mergeOverlappingRanges();

    const gaps: CacheGap[] = [];
    let currentPos = start.getTime();
    const endTime = end.getTime();


    const sortedRanges = this.cachedRanges
      .filter(range => range.end.getTime() >= start.getTime() && range.start.getTime() <= end.getTime())
      .sort((a, b) => a.start.getTime() - b.start.getTime());

    for (const range of sortedRanges) {
      const rangeStart = Math.max(range.start.getTime(), start.getTime());
      const rangeEnd = Math.min(range.end.getTime(), end.getTime());


      if (currentPos < rangeStart) {
        gaps.push({
          start: new Date(currentPos),
          end: new Date(rangeStart)
        });
      }


      range.lastAccessed = Date.now();

      currentPos = Math.max(currentPos, rangeEnd);
    }


    if (currentPos < endTime) {
      gaps.push({
        start: new Date(currentPos),
        end: new Date(endTime)
      });
    }

    return {
      isFullyCovered: gaps.length === 0,
      gaps
    };
  }

  /**
   * Récupérer les items du cache pour une plage donnée
   */
  private getItemsFromCache(start: Date, end: Date): TimelineItem[] {
    const items: TimelineItem[] = [];

    for (const range of this.cachedRanges) {
      if (range.end.getTime() >= start.getTime() && range.start.getTime() <= end.getTime()) {

        const filteredItems = range.items.filter(item => {
          const itemTime = item.start.getTime();
          return itemTime >= start.getTime() && itemTime <= end.getTime();
        });

        items.push(...filteredItems);
        range.lastAccessed = Date.now();
      }
    }

    return items;
  }

  /**
   * Charger seulement les gaps manquants
   */
  private async loadGaps(gaps: CacheGap[]): Promise<void> {
    if (this.isLoadingGaps) {
      console.warn('Already loading gaps, skipping to prevent infinite recursion');
      return;
    }

    this.isLoadingGaps = true;

    try {

      for (const gap of gaps) {
        await this.loadSingleGap(gap.start, gap.end);
      }
    } finally {
      this.isLoadingGaps = false;
    }
  }

  /**
   * Charger un seul gap sans récursion
   */
  private async loadSingleGap(start: Date, end: Date): Promise<void> {
    const rangeKey = this.getRangeKey(start, end);


    if (this.loadingRequests.has(rangeKey)) {
      return;
    }

    this.loadingRequests.add(rangeKey);

    try {
      const url = this.buildLazyLoadUrl(start, end);
      console.log('Loading gap data from URL:', url);

      const response = await fetch(url);
      if (!response.ok) {
        console.error('Gap load failed:', {
          url,
          status: response.status,
          statusText: response.statusText,
          start: start.toISOString(),
          end: end.toISOString()
        });
        throw new Error(`HTTP error! status: ${response.status}`);
      }

      const data = await response.json();
      const records = data.records || data;

      if (Array.isArray(records)) {
        const timelineItems = records.map((record: any) => this.convertToTimelineItem(record));


        await this.addToCache(start, end, timelineItems);


        this.mergeTimelineData(timelineItems);
      }
    } catch (error: any) {
      console.error('Failed to load gap data:', error);
    } finally {
      this.loadingRequests.delete(rangeKey);
    }
  }

  private async preloadAdjacentRanges(): Promise<void> {
    if (!this.timeline || !this.config.lazyLoading?.preloadOnZoom) return;

    const window = this.timeline.getWindow();
    const rangeDuration = window.end.getTime() - window.start.getTime();


    const prevStart = new Date(window.start.getTime() - rangeDuration);
    const prevEnd = new Date(window.start);

    const nextStart = new Date(window.end);
    const nextEnd = new Date(window.end.getTime() + rangeDuration);


    Promise.all([
      this.loadDataForRange(prevStart, prevEnd),
      this.loadDataForRange(nextStart, nextEnd)
    ]).catch(error => {
      console.warn('Preload failed:', error);
    });
  }

  /**
   * Ajouter une nouvelle plage au cache intelligent
   */
  private async addToCache(start: Date, end: Date, items: TimelineItem[]): Promise<void> {
    const size = this.estimateSize(items);
    const now = Date.now();

    const newRange: CachedRange = {
      start,
      end,
      items,
      timestamp: now,
      lastAccessed: now,
      size
    };


    const maxSize = (this.config.lazyLoading?.maxCacheSize || 10) * 1024 * 1024;

    if (this.cacheSize + size > maxSize) {
      await this.evictLRU(size);
    }

    this.cachedRanges.push(newRange);
    this.cacheSize += size;


    this.mergeOverlappingRanges();


    await this.saveCacheToStorage();
  }

  /**
   * Fusionner les plages qui se chevauchent ou sont adjacentes
   */
  private mergeOverlappingRanges(): void {
    if (this.cachedRanges.length <= 1) return;


    this.cachedRanges.sort((a, b) => a.start.getTime() - b.start.getTime());

    const merged: CachedRange[] = [];
    let current = this.cachedRanges[0];

    for (let i = 1; i < this.cachedRanges.length; i++) {
      const next = this.cachedRanges[i];


      if (next.start.getTime() <= current.end.getTime() + (60 * 60 * 1000)) {

        const mergedItems = [...current.items];


        for (const item of next.items) {
          if (!mergedItems.some(existing => existing.id === item.id)) {
            mergedItems.push(item);
          }
        }

        current = {
          start: current.start,
          end: new Date(Math.max(current.end.getTime(), next.end.getTime())),
          items: mergedItems,
          timestamp: Math.min(current.timestamp, next.timestamp),
          lastAccessed: Math.max(current.lastAccessed, next.lastAccessed),
          size: this.estimateSize(mergedItems)
        };
      } else {
        merged.push(current);
        current = next;
      }
    }

    merged.push(current);


    this.cacheSize = merged.reduce((total, range) => total + range.size, 0);
    this.cachedRanges = merged;
  }

  /**
   * Nettoyer le cache expiré
   */
  private cleanExpiredCache(): void {
    const ttl = (this.config.lazyLoading?.cacheTTL || 60) * 60 * 1000;
    const now = Date.now();

    const beforeCount = this.cachedRanges.length;
    this.cachedRanges = this.cachedRanges.filter(range => {
      const isExpired = (now - range.timestamp) > ttl;
      if (isExpired) {
        this.cacheSize -= range.size;
      }
      return !isExpired;
    });

    if (this.cachedRanges.length < beforeCount) {
      console.log(`Cleaned ${beforeCount - this.cachedRanges.length} expired cache entries`);
    }
  }

  /**
   * Éviction LRU (Least Recently Used)
   */
  private async evictLRU(neededSize: number): Promise<void> {

    this.cachedRanges.sort((a, b) => a.lastAccessed - b.lastAccessed);

    let freedSize = 0;
    const toRemove: number[] = [];

    for (let i = 0; i < this.cachedRanges.length && freedSize < neededSize; i++) {
      freedSize += this.cachedRanges[i].size;
      toRemove.push(i);
    }


    for (let i = toRemove.length - 1; i >= 0; i--) {
      const range = this.cachedRanges.splice(toRemove[i], 1)[0];
      this.cacheSize -= range.size;
    }

    console.log(`Evicted ${toRemove.length} cache entries, freed ${freedSize} bytes`);
  }

  /**
   * Estimer la taille d'une liste d'items
   */
  private estimateSize(items: TimelineItem[]): number {
    if (items.length === 0) return 0;


    const sampleSize = Math.min(10, items.length);
    const sample = items.slice(0, sampleSize);
    const sampleJSON = JSON.stringify(sample);

    return Math.ceil((sampleJSON.length * items.length) / sampleSize);
  }

  /**
   * Charger le cache depuis le stockage persistant
   */
  private async loadCacheFromStorage(): Promise<void> {
    const strategy = this.config.lazyLoading?.cacheStrategy || 'memory';

    if (strategy === 'none' || strategy === 'memory') {
      return;
    }

    try {
      let stored: string | null = null;

      if (strategy === 'session') {
        stored = sessionStorage.getItem(`timeline-cache-${this.dataUrl}`);
      } else if (strategy === 'indexeddb') {

        stored = sessionStorage.getItem(`timeline-cache-${this.dataUrl}`);
      }

      if (stored) {
        const parsed = JSON.parse(stored);
        this.cachedRanges = parsed.ranges.map((range: any) => ({
          ...range,
          start: new Date(range.start),
          end: new Date(range.end),
          items: range.items.map((item: any) => ({
            ...item,
            start: new Date(item.start)
          }))
        }));

        this.cacheSize = parsed.cacheSize || this.cachedRanges.reduce((total, range) => total + range.size, 0);


        this.cleanExpiredCache();
      }
    } catch (error) {
      console.warn('Failed to load cache from storage:', error);
      this.cachedRanges = [];
      this.cacheSize = 0;
    }
  }

  /**
   * Sauvegarder le cache dans le stockage persistant
   */
  private async saveCacheToStorage(): Promise<void> {
    const strategy = this.config.lazyLoading?.cacheStrategy || 'memory';

    if (strategy === 'none' || strategy === 'memory') {
      return;
    }

    try {
      const toStore = {
        ranges: this.cachedRanges,
        cacheSize: this.cacheSize,
        timestamp: Date.now()
      };

      const serialized = JSON.stringify(toStore);

      if (strategy === 'session') {
        sessionStorage.setItem(`timeline-cache-${this.dataUrl}`, serialized);
      } else if (strategy === 'indexeddb') {

        sessionStorage.setItem(`timeline-cache-${this.dataUrl}`, serialized);
      }
    } catch (error) {
      console.warn('Failed to save cache to storage:', error);
    }
  }

  /**
   * Invalider une partie du cache (ex: après modification d'une interaction)
   */
  public invalidateCache(start?: Date, end?: Date): void {
    if (!start || !end) {
      this.cachedRanges = [];
      this.cacheSize = 0;
      this.fullDataset = [];
      this.fullDataLoaded = false;
      this.aggregatesLoaded = false;
    } else {
      this.cachedRanges = this.cachedRanges.filter(range => {
        const overlaps = range.end.getTime() >= start.getTime() && range.start.getTime() <= end.getTime();
        if (overlaps) {
          this.cacheSize -= range.size;
        }
        return !overlaps;
      });

      if (this.fullDataLoaded) {
        this.fullDataset = this.fullDataset.filter(item => {
          const itemTime = item.start.getTime();
          return !(itemTime >= start.getTime() && itemTime <= end.getTime());
        });
        this.generateNavigatorAggregatesFromFullData();
      }
    }

    this.saveCacheToStorage();
  }


  get isLoadingData(): boolean {
    return this.isLoading();
  }

  get currentLoadingState(): 'idle' | 'debouncing' | 'loading' {
    return this.loadingState();
  }

  get cacheStats(): { rangeCount: number, totalSize: number, sizeMB: number } {
    return {
      rangeCount: this.cachedRanges.length,
      totalSize: this.cacheSize,
      sizeMB: Math.round((this.cacheSize / (1024 * 1024)) * 100) / 100
    };
  }
}
