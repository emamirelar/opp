import { 
  ChangeDetectionStrategy, 
  Component, 
  ElementRef, 
  EventEmitter, 
  Input, 
  Output, 
  ViewChild, 
  AfterViewInit, 
  OnDestroy, 
  ChangeDetectorRef,
  ContentChildren,
  QueryList,
  TemplateRef,
  ViewChildren,
  signal,
  effect,
  computed,
  untracked
} from '@angular/core';
import { CommonModule } from '@angular/common';

export interface SplitterPanel {
  id: string;
  size?: number;
  minSize?: number;
  maxSize?: number;
  resizable?: boolean;
  visible?: boolean;
  template: TemplateRef<any> | null;
  data?: any;
}

export interface SplitterResizeEvent {
  originalEvent: MouseEvent | TouchEvent;
  sizes: number[];
  panelSizes: number[];
}

@Component({
  selector: 'app-splitter',
  imports: [CommonModule],
  templateUrl: './splitter.component.html',
  styleUrls: ['./splitter.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class SplitterComponent implements AfterViewInit, OnDestroy {
  @Input() layout: 'horizontal' | 'vertical' = 'horizontal';
  @Input() gutterSize: number = 4;
  @Input() step: number = 5;
  @Input() stateStorage: 'session' | 'local' | null = 'session';
  @Input() stateKey: string | null = null;
  @Input() style: { [klass: string]: any } | null = null;
  @Input() styleClass: string | null = null;
  @Input() minSizes: number[] = [];
  @Input() maxSizes: number[] = [];
  @Input() resizerStyle: { [klass: string]: any } | null = null;
  @Input() resizerStyleClass: string | null = null;
  
  // Enhanced inputs for dynamic functionality
  @Input() set panels(value: SplitterPanel[]) {
    if (Array.isArray(value)) {
      // Always update the signal, even with empty array
      this._panels.set([...value]); // Create a copy to ensure change detection
      
    }
  }
  get panels(): SplitterPanel[] {
    return this._panels();
  }

  @Input() set panelSizes(value: number[]) {
    this._panelSizes.set(value);
  }
  get panelSizes(): number[] {
    return this._panelSizes();
  }

  @Output() onResizeStart = new EventEmitter<SplitterResizeEvent>();
  @Output() onResizeEnd = new EventEmitter<SplitterResizeEvent>();
  @Output() onPanelAdd = new EventEmitter<{ panel: SplitterPanel; index: number }>();
  @Output() onPanelRemove = new EventEmitter<{ panel: SplitterPanel; index: number }>();
  @Output() onPanelSizeChange = new EventEmitter<{ sizes: number[]; panels: SplitterPanel[] }>();

  @ViewChild('container', { static: true }) containerViewChild!: ElementRef;
  @ContentChildren(TemplateRef) templates!: QueryList<TemplateRef<any>>;
  @ViewChildren('panel') panelElements!: QueryList<ElementRef>;
  @ViewChildren('gutter') gutterElements!: QueryList<ElementRef>;

  // Signals for reactive state management
  private _panels = signal<SplitterPanel[]>([]);
  private _panelSizes = signal<number[]>([]);
  
  // Computed properties
  visiblePanels = computed(() => this._panels().filter(panel => panel.visible !== false));
  computedSizes = computed(() => {
    const panels = this.visiblePanels();
    const sizes = this._panelSizes();
    
    if (sizes.length === panels.length) {
      return sizes;
    }
    
    // Auto-distribute sizes if not provided
    const equalSize = 100 / panels.length;
    return panels.map(() => equalSize);
  });

  // Internal state
  resizing = false; // Made public for template binding
  private size: number = 0;
  private gutterElement: HTMLElement | null = null;
  private startPos: number = 0;
  private prevPanelElement: HTMLElement | null = null;
  private nextPanelElement: HTMLElement | null = null;
  private prevPanelSize: number = 0;
  private nextPanelSize: number = 0;
  private prevPanelIndex: number = 0;
  private panelSizesState: number[] = [];
  private token: string = '';
  private animationFrameId: number | null = null;

  constructor(
    private cd: ChangeDetectorRef,
    private el: ElementRef
  ) {
    // React to panel changes - use untracked to prevent infinite loops
    effect(() => {
      const panels = this._panels();
      const sizes = this.computedSizes();
      
      if (panels.length > 0 && sizes.length > 0) {
        // Use untracked to prevent triggering another effect cycle
        untracked(() => {
          this.validateAndUpdateSizes(sizes);
        });
      }
    });
  }

  ngAfterViewInit() {
    if (this.panels.length === 0) {
      // Initialize with default panels if none provided
      this.initializeDefaultPanels();
    }
    
    this.restoreState();
  }

  ngOnDestroy() {
    this.unbindMouseListeners();
    this.saveState();
    
    // Clean up animation frame
    if (this.animationFrameId) {
      cancelAnimationFrame(this.animationFrameId);
    }
  }

  // Panel management methods
  addPanel(panel: SplitterPanel, index?: number): void {
    const currentPanels = [...this._panels()];
    
    // Check if panel already exists
    if (currentPanels.some(p => p.id === panel.id)) {
      return;
    }
    
    const insertIndex = index !== undefined ? index : currentPanels.length;
    
    // Ensure panel has required properties
    const newPanel: SplitterPanel = {
      visible: true,
      resizable: true,
      ...panel,
      id: panel.id || this.generatePanelId()
    };
    
    currentPanels.splice(insertIndex, 0, newPanel);
    this._panels.set(currentPanels);
    
    // Recalculate sizes
    this.redistributeSizes();
    
    this.onPanelAdd.emit({ panel: newPanel, index: insertIndex });
  }

  removePanel(panelId: string): void {
    const currentPanels = [...this._panels()];
    const index = currentPanels.findIndex(p => p.id === panelId);
    
    if (index !== -1) {
      const removedPanel = currentPanels[index];
      currentPanels.splice(index, 1);
      this._panels.set(currentPanels);
      
      // Recalculate sizes
      this.redistributeSizes();
      
      this.onPanelRemove.emit({ panel: removedPanel, index });
    }
  }

  updatePanelSize(panelId: string, size: number): void {
    const currentSizes = [...this._panelSizes()];
    const panels = this._panels();
    const index = panels.findIndex(p => p.id === panelId);
    
    if (index !== -1 && index < currentSizes.length) {
      currentSizes[index] = Math.max(0, Math.min(100, size));
      this._panelSizes.set(currentSizes);
      this.validateAndUpdateSizes(currentSizes);
    }
  }

  togglePanel(panelId: string): void {
    const currentPanels = [...this._panels()];
    const panel = currentPanels.find(p => p.id === panelId);
    
    if (panel) {
      panel.visible = !panel.visible;
      this._panels.set(currentPanels);
      this.redistributeSizes();
      
      // Force change detection to update the template
      this.cd.markForCheck();
    }
  }

  // Resizing functionality (similar to PrimeNG)
  onGutterMouseDown(event: MouseEvent, index: number) {
    
    this.size = this.layout === 'horizontal' ? this.getWidth(this.containerViewChild.nativeElement) : this.getHeight(this.containerViewChild.nativeElement);
    this.gutterElement = event.currentTarget as HTMLElement;
    this.startPos = this.layout === 'horizontal' ? event.pageX : event.pageY;
    this.prevPanelElement = this.gutterElement.previousElementSibling as HTMLElement;
    this.nextPanelElement = this.gutterElement.nextElementSibling as HTMLElement;
    
    // Get current computed sizes from the DOM
    const prevRect = this.prevPanelElement.getBoundingClientRect();
    const nextRect = this.nextPanelElement.getBoundingClientRect();
    const containerRect = this.containerViewChild.nativeElement.getBoundingClientRect();
    
    if (this.layout === 'horizontal') {
      this.prevPanelSize = (prevRect.width / containerRect.width) * 100;
      this.nextPanelSize = (nextRect.width / containerRect.width) * 100;
    } else {
      this.prevPanelSize = (prevRect.height / containerRect.height) * 100;
      this.nextPanelSize = (nextRect.height / containerRect.height) * 100;
    }
    
    this.prevPanelIndex = index;
    this.token = this.generateToken();
    this.resizing = true;
    
    // Initialize panel sizes state if not already done
    if (this.panelSizesState.length === 0) {
      this.panelSizesState = [...this.computedSizes()];
    }

    this.onResizeStart.emit({
      originalEvent: event,
      sizes: this.panelSizesState,
      panelSizes: this.computedSizes()
    });

    this.bindMouseListeners();
    this.cd.markForCheck();
  }

  onGutterTouchStart(event: TouchEvent, index: number) {
    const touch = event.changedTouches[0];
    this.onGutterMouseDown({
      ...touch,
      pageX: touch.pageX,
      pageY: touch.pageY,
      currentTarget: event.currentTarget
    } as any, index);
  }

  // Private methods
  private initializeDefaultPanels(): void {
    // Create default panels from templates if available
    if (this.templates && this.templates.length > 0) {
      const defaultPanels: SplitterPanel[] = this.templates.map((template, index) => ({
        id: this.generatePanelId(),
        size: 100 / this.templates.length,
        visible: true,
        resizable: true,
        template: template
      }));
      
      this._panels.set(defaultPanels);
    }
  }

  private validateAndUpdateSizes(sizes: number[]): void {
    const panels = this.visiblePanels();
    
    if (sizes.length !== panels.length) {
      return;
    }

    // Ensure sizes sum to 100%
    const total = sizes.reduce((sum, size) => sum + size, 0);
    const normalizedSizes = total > 0 ? sizes.map(size => (size / total) * 100) : sizes;
    
    // Only update if sizes have actually changed to prevent infinite loops
    const sizesChanged = !this.arraysEqual(this.panelSizesState, normalizedSizes);
    
    if (sizesChanged) {
      this.panelSizesState = normalizedSizes;
      this.onPanelSizeChange.emit({ sizes: normalizedSizes, panels });
      this.cd.markForCheck();
    }
  }

  private arraysEqual(a: number[], b: number[]): boolean {
    if (a.length !== b.length) return false;
    return a.every((val, index) => Math.abs(val - b[index]) < 0.001);
  }

  private redistributeSizes(): void {
    const panels = this.visiblePanels();
    
    if (panels.length === 0) {
      return;
    }
    
    const equalSize = 100 / panels.length;
    const newSizes = panels.map(() => equalSize);
    
    // Only update if sizes have changed
    if (!this.arraysEqual(this._panelSizes(), newSizes)) {
      this._panelSizes.set(newSizes);
      this.validateAndUpdateSizes(newSizes);
    }
  }

  private generatePanelId(): string {
    return `panel-${Date.now()}-${Math.random().toString(36).substr(2, 9)}`;
  }

  private generateToken(): string {
    return Math.random().toString(36).substr(2, 9);
  }

  // Mouse event handlers
  private bindMouseListeners(): void {
    if (!this.mouseMoveListener) {
      this.mouseMoveListener = this.onMouseMove.bind(this);
      document.addEventListener('mousemove', this.mouseMoveListener);
    }

    if (!this.mouseUpListener) {
      this.mouseUpListener = this.onMouseUp.bind(this);
      document.addEventListener('mouseup', this.mouseUpListener);
    }

    if (!this.touchMoveListener) {
      this.touchMoveListener = this.onTouchMove.bind(this);
      document.addEventListener('touchmove', this.touchMoveListener);
    }

    if (!this.touchEndListener) {
      this.touchEndListener = this.onTouchEnd.bind(this);
      document.addEventListener('touchend', this.touchEndListener);
    }
  }

  private unbindMouseListeners(): void {
    if (this.mouseMoveListener) {
      document.removeEventListener('mousemove', this.mouseMoveListener);
      this.mouseMoveListener = null;
    }

    if (this.mouseUpListener) {
      document.removeEventListener('mouseup', this.mouseUpListener);
      this.mouseUpListener = null;
    }

    if (this.touchMoveListener) {
      document.removeEventListener('touchmove', this.touchMoveListener);
      this.touchMoveListener = null;
    }

    if (this.touchEndListener) {
      document.removeEventListener('touchend', this.touchEndListener);
      this.touchEndListener = null;
    }
  }

  private mouseMoveListener: ((event: MouseEvent) => void) | null = null;
  private mouseUpListener: ((event: MouseEvent) => void) | null = null;
  private touchMoveListener: ((event: TouchEvent) => void) | null = null;
  private touchEndListener: ((event: TouchEvent) => void) | null = null;

  private onMouseMove(event: MouseEvent): void {
    if (!this.resizing) return;
    
    // Cancel any pending animation frame
    if (this.animationFrameId) {
      cancelAnimationFrame(this.animationFrameId);
    }
    
    // Use requestAnimationFrame for smooth updates
    this.animationFrameId = requestAnimationFrame(() => {
      const newPos = this.layout === 'horizontal' ? event.pageX : event.pageY;
      const deltaPos = newPos - this.startPos;
      const deltaPercentage = (deltaPos * 100) / this.size;
      
      const newPrevPanelSize = this.prevPanelSize + deltaPercentage;
      const newNextPanelSize = this.nextPanelSize - deltaPercentage;

      if (this.validateResize(newPrevPanelSize, newNextPanelSize)) {
        const visiblePanelsCount = this.visiblePanels().length;
        const gutterOffset = ((visiblePanelsCount - 1) * this.gutterSize) / visiblePanelsCount;
        
        // Update the DOM directly for immediate visual feedback
        this.prevPanelElement!.style.flexBasis = `calc(${newPrevPanelSize}% - ${gutterOffset}px)`;
        this.nextPanelElement!.style.flexBasis = `calc(${newNextPanelSize}% - ${gutterOffset}px)`;
        
        // Update internal state
        this.panelSizesState[this.prevPanelIndex] = newPrevPanelSize;
        this.panelSizesState[this.prevPanelIndex + 1] = newNextPanelSize;
      }
      
      this.animationFrameId = null;
    });
  }

  private onTouchMove(event: TouchEvent): void {
    const touch = event.changedTouches[0];
    this.onMouseMove({
      pageX: touch.pageX,
      pageY: touch.pageY
    } as MouseEvent);
  }

  private onMouseUp(event: MouseEvent): void {
    if (this.resizing) {
      // Cancel any pending animation frame
      if (this.animationFrameId) {
        cancelAnimationFrame(this.animationFrameId);
        this.animationFrameId = null;
      }
      
      this.resizing = false;
      
      // Update the reactive state with final sizes
      this._panelSizes.set([...this.panelSizesState]);
      
      this.onResizeEnd.emit({
        originalEvent: event,
        sizes: this.panelSizesState,
        panelSizes: this.computedSizes()
      });
      
      this.unbindMouseListeners();
      this.saveState();
      this.cd.markForCheck();
    }
  }

  private onTouchEnd(event: TouchEvent): void {
    const touch = event.changedTouches[0];
    this.onMouseUp({
      ...touch
    } as any);
  }

  private validateResize(newPrevPanelSize: number, newNextPanelSize: number): boolean {
    const prevMinSize = this.minSizes[this.prevPanelIndex] || 0;
    const nextMinSize = this.minSizes[this.prevPanelIndex + 1] || 0;
    const prevMaxSize = this.maxSizes[this.prevPanelIndex] || 100;
    const nextMaxSize = this.maxSizes[this.prevPanelIndex + 1] || 100;

    return newPrevPanelSize >= prevMinSize && newPrevPanelSize <= prevMaxSize &&
           newNextPanelSize >= nextMinSize && newNextPanelSize <= nextMaxSize;
  }

  // State management
  private saveState(): void {
    if (this.stateStorage && this.stateKey) {
      const storage = this.stateStorage === 'session' ? sessionStorage : localStorage;
      storage.setItem(this.stateKey, JSON.stringify(this.panelSizesState));
    }
  }

  private restoreState(): void {
    if (this.stateStorage && this.stateKey) {
      const storage = this.stateStorage === 'session' ? sessionStorage : localStorage;
      const stateValue = storage.getItem(this.stateKey);
      
      if (stateValue) {
        try {
          const savedSizes = JSON.parse(stateValue);
          if (Array.isArray(savedSizes)) {
            this._panelSizes.set(savedSizes);
          }
        } catch (e) {
          // Ignore parsing errors
        }
      }
    }
  }

  // Track by functions for ngFor optimization
  trackByPanelId(index: number, panel: SplitterPanel): string {
    return panel.id;
  }

  // Check if gutter should be shown between panels
  shouldShowGutter(index: number): boolean {
    const sizes = this.computedSizes();
    const visiblePanels = this.visiblePanels();
    
    // Only show gutter if both current and next panels have non-zero size
    if (index >= 0 && index < visiblePanels.length - 1 && index < sizes.length - 1) {
      const currentPanelSize = sizes[index] || 0;
      const nextPanelSize = sizes[index + 1] || 0;
      
      // Show gutter only if both panels have size > 0
      return currentPanelSize > 0 && nextPanelSize > 0;
    }
    
    return false;
  }

  // Get computed gutter style
  getGutterStyle(): { [key: string]: string } {
    return {
      ...(this.resizerStyle || {}),
      width: this.layout === 'horizontal' ? this.gutterSize + 'px' : '100%',
      height: this.layout === 'vertical' ? this.gutterSize + 'px' : '100%'
    };
  }

  // Helper method for demo/testing
  addSamplePanel(): void {
    const newPanel: SplitterPanel = {
      id: this.generatePanelId(),
      visible: true,
      resizable: true,
      template: null,
      data: { title: `Panel ${this.panels.length + 1}` }
    };
    
    this.addPanel(newPanel);
  }

  // Force refresh of panels and sizes
  refreshPanels(): void {
    const currentPanels = this._panels();
    if (currentPanels.length > 0) {
      // Trigger a new array reference to force reactivity
      this._panels.set([...currentPanels]);
      this.redistributeSizes();
      this.cd.markForCheck();
    }
  }

  // Utility methods
  private getWidth(el: HTMLElement): number {
    return el.offsetWidth;
  }

  private getHeight(el: HTMLElement): number {
    return el.offsetHeight;
  }

  private getOuterWidth(el: HTMLElement, margin: boolean = false): number {
    let width = el.offsetWidth;
    
    if (margin) {
      const style = getComputedStyle(el);
      width += parseFloat(style.marginLeft) + parseFloat(style.marginRight);
    }
    
    return width;
  }

  private getOuterHeight(el: HTMLElement, margin: boolean = false): number {
    let height = el.offsetHeight;
    
    if (margin) {
      const style = getComputedStyle(el);
      height += parseFloat(style.marginTop) + parseFloat(style.marginBottom);
    }
    
    return height;
  }
}
