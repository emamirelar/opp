import { Component, Input, OnInit, OnChanges, SimpleChanges, ViewEncapsulation, inject, PLATFORM_ID, signal, output, ViewChild, ElementRef, AfterViewInit, Output, EventEmitter, ChangeDetectionStrategy, OnDestroy } from '@angular/core';
import { CommonModule, isPlatformBrowser } from '@angular/common';
import { MarkdownModule } from 'ngx-markdown';
import { ResultItem } from '../ai-assistant.model';
import { EntityGridComponent } from './entity-grid/entity-grid.component';
import { ChartJsComponent } from './chart-js/chart-js.component';

@Component({
  selector: 'app-content-renderer',
  standalone: true,
  imports: [CommonModule, MarkdownModule, EntityGridComponent, ChartJsComponent],
  templateUrl: './content-renderer.component.html',
  styleUrls: ['./content-renderer.component.css'],
  encapsulation: ViewEncapsulation.None,
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ContentRendererComponent implements OnInit, OnChanges, AfterViewInit, OnDestroy {
  @Input() item!: ResultItem;
  @Input() shouldShow: boolean = true; // Controls when this item should be visible
  @Input() isSequential: boolean = false; // Whether this is part of sequential display
  @Input() isNewMessage: boolean = true; // Whether this is a new message (for typewriter effect)
  @Input() renderingId?: string; // Unique identifier for progressive rendering
  @Input() isProgressive: boolean = false; // Whether this is progressive content
  
  // Output when this content item is done displaying
  contentComplete = output<void>();
  @Output() cardClicked = new EventEmitter<any>();

  // Track previous content for change detection
  private previousContent: string = '';
  
  // Add component instance ID for debugging
  private instanceId = Math.random().toString(36).substr(2, 9);
  
  private platformId = inject(PLATFORM_ID);
  private isBrowser = isPlatformBrowser(this.platformId);

  @ViewChild('mermaidElement') mermaidElement?: ElementRef<HTMLDivElement>;

  ngOnChanges(changes: SimpleChanges): void {
    // Debug what changes are triggering this
    console.log('🎨 ContentRenderer - ngOnChanges triggered:', {
      instanceId: this.instanceId,
      type: this.item?.type,
      renderingId: this.renderingId,
      completed: this.item?.completed,
      changedProperties: Object.keys(changes),
      isFirstChange: changes['item']?.firstChange,
      itemReference: this.item === changes['item']?.previousValue ? 'SAME' : 'DIFFERENT',
      currentObjectRef: this.item,
      previousObjectRef: changes['item']?.previousValue
    });
    
    // Check if the item content has changed for progressive rendering
    if (changes['item'] && !changes['item'].firstChange) {
      const currentContent = this.getStringMessage();
      
      // AGGRESSIVE: For completed cards, ignore ALL changes - they should be frozen
      if (this.item.type === 'card' && this.item.completed === true) {
        // Check if it's truly the same object reference (should be with our new freezing logic)
        const sameObjectReference = this.item === changes['item'].previousValue;
        
        if (sameObjectReference) {
          console.log('🔒 ContentRenderer - IGNORING change for completed card: same object reference (frozen):', {
            renderingId: this.renderingId,
            sameReference: true
          });
          return; // Skip ALL processing for completed cards with same reference
        }
        
        // Fallback content comparison if somehow references differ
        const contentChanged = JSON.stringify(this.item.message) !== JSON.stringify(changes['item'].previousValue?.message);
        if (!contentChanged) {
          console.log('🔒 ContentRenderer - IGNORING change for completed card: content unchanged:', {
            renderingId: this.renderingId,
            sameReference: false,
            contentChanged: false
          });
          return; // Skip re-rendering if card is completed and content hasn't changed
        } else {
          console.log('🚨 ContentRenderer - UNEXPECTED: completed card content changed (should not happen):', {
            renderingId: this.renderingId,
            sameReference: false,
            contentChanged: true
          });
        }
      }
      
      if (this.isProgressive && currentContent !== this.previousContent) {
        console.log('🎨 ContentRenderer - Progressive content updated:', {
          renderingId: this.renderingId,
          contentLength: currentContent.length,
          itemType: this.item.type,
          completed: this.item.completed
        });
        
        this.previousContent = currentContent;
        
        // Re-render mermaid diagrams if type is mermaid and content changed
        if (this.item.type === 'mermaid' && this.isBrowser) {
          setTimeout(() => this.renderMermaidDiagram(), 10);
        }
      }
    }
  }

  async ngOnInit() {
    console.log('🎨 ContentRenderer - INITIALIZING new component:', {
      instanceId: this.instanceId,
      type: this.item.type,
      entity: this.item.entity,
      renderingId: this.renderingId,
      completed: this.item.completed,
      messageLength: Array.isArray(this.item.message) ? this.item.message.length : 'N/A',
      isNewMessage: this.isNewMessage,
      isProgressive: this.isProgressive,
      objectReference: this.item
    });
    
    // Initialize previous content for change detection
    this.previousContent = this.getStringMessage();
    
    // Emit content complete immediately - no delays or animations
    if (this.shouldShow) {
      // Use setTimeout with 0 delay to ensure it happens after current execution stack
      setTimeout(() => {
        this.emitContentComplete();
      }, 0);
    }
  }

  async ngAfterViewInit() {
    if (this.item.type === 'mermaid' && this.isBrowser) {
      // Small delay to ensure element is ready
      setTimeout(() => this.renderMermaidDiagram(), 100);
    }
  }

  getStringMessage(): string {
    if (typeof this.item.message === 'string') {
      return this.item.message;
    }
    // Fallback for array or other types
    return JSON.stringify(this.item.message);
  }

  getContentTypeLabel(): string {
    switch (this.item.type) {
      case 'chartjs':
        return this.getChartTypeLabel();
      case 'mermaid':
        return 'Mermaid Diagram';
      case 'code':
        return this.item.language ? this.item.language.toUpperCase() : 'Code';
      case 'grid':
        return this.item.entity ? `${this.item.entity} Table` : 'Data Table';
      case 'card':
        return this.item.entity ? `${this.item.entity} Cards` : 'Data Cards';
      case 'thought':
        return 'AI Thought Process';
      default:
        return this.item.type?.charAt(0).toUpperCase() + this.item.type?.slice(1) || 'Content';
    }
  }

  getArrayMessage(): any[] {
    console.log('🎨 ContentRenderer - getArrayMessage called for type:', this.item.type);
    console.log('🎨 ContentRenderer - message is array:', Array.isArray(this.item.message));
    console.log('🎨 ContentRenderer - message value:', this.item.message);
    
    // Handle array of objects (multiple cards/items) - return as-is
    if (Array.isArray(this.item.message)) {
      console.log('🎨 ContentRenderer - Multiple items: returning array with', this.item.message.length, 'items');
      return this.item.message;
    }
    
    // Handle single object (single card/item) - wrap in array for consistent display
    if (this.item.message && typeof this.item.message === 'object') {
      console.log('🎨 ContentRenderer - Single item: converting object to array');
      return [this.item.message];
    }
    
    // Fallback for string or other types - return empty array
    console.log('🎨 ContentRenderer - Returning empty array (fallback)');
    return [];
  }

  // Content is shown immediately, no typing effect
  private emitContentComplete(): void {
    this.contentComplete.emit();
  }

  private isNonTextContent(): boolean {
    return ['grid', 'card', 'mermaid', 'code', 'chartjs'].includes(this.item.type);
  }

  // Chart.js related methods
  getChartType(): string {
    return (this.item as any).chartType || 'pie';
  }

  getChartConfig(): any {
    if (typeof this.item.message === 'object' && this.item.message !== null) {
      return this.item.message;
    }
    return null;
  }

  getChartData(): any {
    if (typeof this.item.message === 'object' && this.item.message !== null) {
      return (this.item.message as any).data;
    }
    return null;
  }

  getChartTypeLabel(): string {
    const chartType = this.getChartType();
    const typeLabels: { [key: string]: string } = {
      'pie': 'Pie Chart',
      'doughnut': 'Doughnut Chart',
      'bar': 'Bar Chart',
      'line': 'Line Chart',
      'radar': 'Radar Chart',
      'polar': 'Polar Chart',
      'scatter': 'Scatter Plot'
    };
    
    return typeLabels[chartType] || `${chartType.charAt(0).toUpperCase() + chartType.slice(1)} Chart`;
  }

  private async renderMermaidDiagram(): Promise<void> {
    if (!this.mermaidElement) {
      console.warn('🎨 Mermaid element not ready for progressive rendering');
      return;
    }

    try {
      const mermaid = await import('mermaid');
      
      // Configure mermaid
      mermaid.default.initialize({ 
        startOnLoad: false,
        theme: 'default',
        securityLevel: 'loose',
        fontFamily: 'arial'
      });

      // Generate unique ID for this diagram (include renderingId if available)
      const diagramId = this.renderingId 
        ? `mermaid-${this.renderingId}-${Date.now()}`
        : `mermaid-${Date.now()}-${Math.random().toString(36).substr(2, 9)}`;
      
      // Render the diagram with proper newline handling
      let diagramCode = this.getStringMessage();
      
      // Convert escaped newlines to actual newlines for proper Mermaid parsing
      diagramCode = diagramCode.replace(/\\n/g, '\n');
      
      console.log('🎨 Rendering/updating mermaid diagram:', {
        renderingId: this.renderingId,
        isProgressive: this.isProgressive,
        diagramId: diagramId,
        contentLength: diagramCode.length
      });
      
      const { svg } = await mermaid.default.render(diagramId, diagramCode);
      
      // Insert the rendered SVG
      if (this.mermaidElement) {
        this.mermaidElement.nativeElement.innerHTML = svg;
      }
      
      console.log('🎨 Mermaid diagram rendered successfully');
    } catch (error) {
      console.error('🎨 Failed to render mermaid diagram:', error);
      // Fallback: show the raw mermaid code
      if (this.mermaidElement) {
        this.mermaidElement.nativeElement.innerHTML = `<pre><code>${this.getStringMessage()}</code></pre>`;
      }
    }
  }
  
  ngOnDestroy(): void {
    console.log('💥 ContentRenderer - DESTROYING component:', {
      instanceId: this.instanceId,
      type: this.item?.type,
      renderingId: this.renderingId,
      completed: this.item?.completed
    });
  }
} 