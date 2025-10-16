import { Component, Input, OnInit, OnChanges, SimpleChanges, ViewEncapsulation, inject, PLATFORM_ID, signal, output, ViewChild, ElementRef, AfterViewInit, Output, EventEmitter, ChangeDetectionStrategy, OnDestroy } from '@angular/core';
import { CommonModule, isPlatformBrowser } from '@angular/common';
import { MarkdownModule } from 'ngx-markdown';
import { ContentPart } from '../ai-assistant.model';
import { EntityGridComponent } from './entity-grid/entity-grid.component';
import { ChartJsComponent } from './chart-js/chart-js.component';
import { CollapsibleThoughtComponent } from './collapsible-thought/collapsible-thought.component';

@Component({
  selector: 'app-content-renderer',
  standalone: true,
  imports: [CommonModule, MarkdownModule, EntityGridComponent, ChartJsComponent, CollapsibleThoughtComponent],
  templateUrl: './content-renderer.component.html',
  styleUrls: ['./content-renderer.component.css'],
  encapsulation: ViewEncapsulation.None,
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ContentRendererComponent implements OnInit, OnChanges, AfterViewInit, OnDestroy {
  @Input() item!: ContentPart;
  @Input() shouldShow: boolean = true; // Controls when this item should be visible
  @Input() isSequential: boolean = false; // Whether this is part of sequential display
  @Input() isNewMessage: boolean = true; // Whether this is a new message (for typewriter effect)
  @Input() renderingId?: string; // Unique identifier for progressive rendering
  @Input() isProgressive: boolean = false; // Whether this is progressive content
  @Input() isUserMessage: boolean = false; // Whether this is a user message (for styling)
  
  private timeoutIds: (number | ReturnType<typeof setTimeout>)[] = []; // Track timeout IDs for cleanup
  
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
    
    // Check if the item content has changed for progressive rendering
    if (changes['item'] && !changes['item'].firstChange) {
      const currentContent = this.getStringMessage();
      
      
      // AGGRESSIVE: For completed cards, ignore ALL changes - they should be frozen
      if (this.item.type === 'card' && this.item.completed === true) {
        // Check if it's truly the same object reference (should be with our new freezing logic)
        const sameObjectReference = this.item === changes['item'].previousValue;
        
        if (sameObjectReference) {
          return; // Skip ALL processing for completed cards with same reference
        }
        
        // Fallback content comparison if somehow references differ
        const currentContent = JSON.stringify(this.item.entity || this.item.text || this.item.functionCall || this.item.functionResponse);
        const previousContent = JSON.stringify(changes['item'].previousValue?.entity || changes['item'].previousValue?.text || changes['item'].previousValue?.functionCall || changes['item'].previousValue?.functionResponse);
        const contentChanged = currentContent !== previousContent;
        if (!contentChanged) {
          return; // Skip re-rendering if card is completed and content hasn't changed
        }
      }
      
      // CRITICAL FIX: Process content changes regardless of isProgressive state
      // The final chunk might have isProgressive=false but still needs to be rendered
      if (currentContent !== this.previousContent) {
        this.previousContent = currentContent;
        
        // Re-render mermaid diagrams if type is mermaid and content changed
        if (this.item.type === 'mermaid' && this.isBrowser) {
          setTimeout(() => this.renderMermaidDiagram(), 10);
        }
      } else {
      }
    }
  }

  async ngOnInit() {
    // Initialize previous content for change detection
    this.previousContent = this.getStringMessage();
    
    // Emit content complete immediately - no delays or animations
    if (this.shouldShow) {
      // Use setTimeout with 0 delay to ensure it happens after current execution stack
      const timeoutId = setTimeout(() => {
        this.emitContentComplete();
      }, 0);
      this.timeoutIds.push(timeoutId);
    }
  }

  async ngAfterViewInit() {
    if (this.item.type === 'mermaid' && this.isBrowser) {
      // Small delay to ensure element is ready
      const timeoutId = setTimeout(() => this.renderMermaidDiagram(), 100);
      this.timeoutIds.push(timeoutId);
    }
  }

  getStringMessage(): string {
    if (this.item.text) {
      return this.item.text;
    }
    
    // Handle function calls and responses - format them nicely
    if (this.item.type === 'functionCall' && this.item.functionCall) {
      return JSON.stringify(this.item.functionCall, null, 2);
    }
    
    if (this.item.type === 'functionResponse' && this.item.functionResponse) {
      return JSON.stringify(this.item.functionResponse, null, 2);
    }
    
    // Fallback for entity data
    if (this.item.entity) {
      return JSON.stringify(this.item.entity, null, 2);
    }
    
    return '';
  }

  getContentTypeLabel(): string {
    switch (this.item.type) {
      case 'chartjs':
        return this.getChartTypeLabel();
      case 'chart':
        return this.getChartTypeLabel();
      case 'mermaid':
        return 'Mermaid Diagram';
      case 'code':
        return 'Code';
      case 'grid':
        return this.item.entityType ? `${this.item.entityType} Table` : 'Data Table';
      case 'card':
        return this.item.entityType ? `${this.item.entityType} Cards` : 'Data Cards';
      case 'thought':
      case 'thoughts':
        return 'AI Thought Process';
      case 'functionCall':
        return 'Function Call';
      case 'functionResponse':
        return 'Function Response';
      default:
        return this.item.type ? this.item.type.charAt(0).toUpperCase() + this.item.type.slice(1) : 'Content';
    }
  }

  getArrayMessage(): any[] {
    // Handle entity data (grid/card components)
    if (this.item.entity && Array.isArray(this.item.entity)) {
      return this.item.entity;
    }
    
    // Handle single entity object - wrap in array for consistent display
    if (this.item.entity && typeof this.item.entity === 'object') {
      return [this.item.entity];
    }
    
    // Fallback - return empty array
    return [];
  }

  // Content is shown immediately, no typing effect
  private emitContentComplete(): void {
    // Only emit if component is still alive (timeoutIds array exists)
    if (this.timeoutIds !== null) {
      this.contentComplete.emit();
    }
  }

  private isNonTextContent(): boolean {
    return this.item.type ? ['grid', 'card', 'mermaid', 'code', 'chartjs'].includes(this.item.type) : false;
  }

  // Chart.js related methods
  getChartType(): string {
    return (this.item as any).chartType || 'pie';
  }

  getChartConfig(): any {
    if (this.item.entity && typeof this.item.entity === 'object') {
      return this.item.entity;
    }
    return null;
  }

  getChartData(): any {
    if (this.item.entity && typeof this.item.entity === 'object') {
      return (this.item.entity as any).data;
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
      
      const { svg } = await mermaid.default.render(diagramId, diagramCode);
      
      // Insert the rendered SVG
      if (this.mermaidElement) {
        this.mermaidElement.nativeElement.innerHTML = svg;
      }
    } catch (error) {
      // Fallback: show the raw mermaid code
      if (this.mermaidElement) {
        this.mermaidElement.nativeElement.innerHTML = `<pre><code>${this.getStringMessage()}</code></pre>`;
      }
    }
  }
  
  ngOnDestroy(): void {
    // Clear all pending timeouts to prevent memory leaks and destroyed component emissions
    this.timeoutIds.forEach(timeoutId => clearTimeout(timeoutId));
    this.timeoutIds = [];
  }
} 
